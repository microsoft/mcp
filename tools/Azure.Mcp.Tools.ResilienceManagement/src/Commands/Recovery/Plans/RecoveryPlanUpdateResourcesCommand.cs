// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "ace3cbba-d572-47dc-a452-ab2cb349e17b",
    Name = "update",
    Title = "Update Resilience Recovery Plan Resources",
    Description = """
        Updates recovery resources in a resilience recovery plan in an Azure service group: includes and configures a resource
        with protection settings; keeps a resource in the plan but excludes it from recovery operations; or removes a resource
        from membership while retaining the plan and all other resources. Supports CustomRunbook with failover and reprotect
        runbooks. Supports AzureSiteRecovery only for virtual machines with disk reprotection, staging storage, and a test
        failover virtual network.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanUpdateResourcesCommand(ILogger<RecoveryPlanUpdateResourcesCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanUpdateResourcesOptions, RecoveryPlanUpdateResourcesCommand.RecoveryPlanUpdateResourcesCommandResult>
{
    private const int MaxPayloadLength = 1_048_576;
    private readonly ILogger<RecoveryPlanUpdateResourcesCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanUpdateResourcesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);

        try
        {
            _ = CreateContent(options);
        }
        catch (ArgumentException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanUpdateResourcesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            UpdateRecoveryResourcesContent content = CreateContent(options);
            var result = await _resilienceManagementService.UpdateRecoveryPlanResourcesAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                content,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new RecoveryPlanUpdateResourcesCommandResult(result),
                ResilienceManagementJsonContext.Default.RecoveryPlanUpdateResourcesCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error updating recovery plan resources. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}.",
                options.ServiceGroup, options.RecoveryPlan);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal static UpdateRecoveryResourcesContent CreateContent(RecoveryPlanUpdateResourcesOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ResourcesToUpdate) && string.IsNullOrWhiteSpace(options.ResourcesToRemove))
        {
            throw new ArgumentException("Specify at least one of --resources-to-update or --resources-to-remove.");
        }

        if ((options.ResourcesToUpdate is { } updatesJson && Encoding.UTF8.GetByteCount(updatesJson) > MaxPayloadLength) ||
            (options.ResourcesToRemove is { } removalsJson && Encoding.UTF8.GetByteCount(removalsJson) > MaxPayloadLength))
        {
            throw new ArgumentException("Each recovery resource JSON payload must not exceed 1 MB.");
        }

        try
        {
            using JsonDocument updates = JsonDocument.Parse(options.ResourcesToUpdate ?? "[]");
            using JsonDocument removals = JsonDocument.Parse(options.ResourcesToRemove ?? "[]");
            ValidateResourceArrays(options, updates.RootElement, removals.RootElement);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("resourcesToUpdate");
                updates.RootElement.WriteTo(writer);
                writer.WritePropertyName("resourcesToRemove");
                removals.RootElement.WriteTo(writer);
                writer.WriteEndObject();
            }

            var reader = new Utf8JsonReader(stream.ToArray());
            var model = new UpdateRecoveryResourcesContent();
            return ((IJsonModel<UpdateRecoveryResourcesContent>)model).Create(
                ref reader,
                ModelReaderWriterOptions.Json) ??
                throw new ArgumentException("The recovery resource configuration could not be parsed.");
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Recovery resource inputs must be valid JSON arrays.", ex);
        }
    }

    private static void ValidateResourceArrays(RecoveryPlanUpdateResourcesOptions options, JsonElement updates, JsonElement removals)
    {
        if (updates.ValueKind != JsonValueKind.Array || removals.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("Recovery resource inputs must be JSON arrays.");
        }

        var updatedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement update in updates.EnumerateArray())
        {
            if (update.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Each resource in --resources-to-update must be an object.");
            }

            if (!update.TryGetProperty("properties", out JsonElement properties) ||
                properties.ValueKind != JsonValueKind.Object ||
                !properties.TryGetProperty("recoveryResourceUniqueId", out JsonElement uniqueIdElement) ||
                uniqueIdElement.ValueKind != JsonValueKind.String ||
                !Guid.TryParse(uniqueIdElement.GetString(), out Guid uniqueId))
            {
                throw new ArgumentException("Each resource in --resources-to-update must contain a properties.recoveryResourceUniqueId GUID.");
            }

            string id = CreateRecoveryResourceId(options.ServiceGroup, options.RecoveryPlan, uniqueId.ToString());
            if (update.TryGetProperty("id", out JsonElement idElement) &&
                (idElement.ValueKind != JsonValueKind.String ||
                 !string.Equals(idElement.GetString(), id, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("A supplied recovery resource id must match properties.recoveryResourceUniqueId and belong to the selected recovery plan.");
            }

            if (!updatedIds.Add(id))
            {
                throw new ArgumentException($"Recovery resource '{id}' appears more than once in --resources-to-update.");
            }

            if (properties.TryGetProperty("inclusionState", out JsonElement inclusionState) &&
                inclusionState.ValueKind != JsonValueKind.Null &&
                (inclusionState.ValueKind != JsonValueKind.String ||
                 inclusionState.GetString() is not ("Included" or "Excluded")))
            {
                throw new ArgumentException("A recovery resource inclusionState must be Included, Excluded, or null.");
            }
        }

        var removedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (JsonElement removal in removals.EnumerateArray())
        {
            if (removal.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException("Each value in --resources-to-remove must be a recovery-resource ID string.");
            }

            string id = removal.GetString()!;
            ValidateRecoveryResourceId(id, options.ServiceGroup, options.RecoveryPlan);
            if (!removedIds.Add(id))
            {
                throw new ArgumentException($"Recovery resource '{id}' appears more than once in --resources-to-remove.");
            }

            if (updatedIds.Contains(id))
            {
                throw new ArgumentException($"Recovery resource '{id}' cannot be updated and removed in the same request.");
            }
        }
    }

    private static string CreateRecoveryResourceId(string serviceGroup, string recoveryPlan, string recoveryResourceUniqueId) =>
        $"/providers/Microsoft.Management/serviceGroups/{serviceGroup}/providers/Microsoft.AzureResilienceManagement/recoveryPlans/{recoveryPlan}/recoveryResources/{recoveryResourceUniqueId}";

    private static void ValidateRecoveryResourceId(string id, string serviceGroup, string recoveryPlan)
    {
        string expectedPrefix = CreateRecoveryResourceId(serviceGroup, recoveryPlan, string.Empty);
        if (!id.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase) ||
            id.Length == expectedPrefix.Length ||
            id.AsSpan(expectedPrefix.Length).Contains('/'))
        {
            throw new ArgumentException($"Recovery resource IDs must belong to service group '{serviceGroup}' and recovery plan '{recoveryPlan}'.");
        }
    }

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argumentException => argumentException.Message,
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "Recovery plan resources cannot be updated while another recovery operation is in progress.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed updating recovery plan resources. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Recovery plan not found. Verify the recovery plan and service group exist and you have access.",
        RequestFailedException =>
            "The recovery plan resource update failed. Verify the resource IDs and protection settings, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record RecoveryPlanUpdateResourcesCommandResult(RecoveryPlanUpdateResourcesResult Result);
}
