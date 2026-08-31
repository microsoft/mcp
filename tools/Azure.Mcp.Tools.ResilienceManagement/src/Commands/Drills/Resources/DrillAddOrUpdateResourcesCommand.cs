// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Drills.Resources;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager.ResilienceManagement.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Drills.Resources;

[CommandMetadata(
    Id = "3f0c8d4e-6b2a-4f9d-8f1c-2a7b6d1e94c5",
    Name = "add-or-update",
    Title = "Add or Update Resilience Drill Resources",
    Description = """
        Adds a resource to a resilience drill, or updates or excludes existing drill resources, in an Azure service group.
        Provide a fault duration in minutes and the Azure resource IDs to include, update, or exclude. Use this to add a
        resource to a drill, change the fault settings on a drill resource, or remove a resource from the drill. It starts
        the operation and returns the operation ID.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DrillAddOrUpdateResourcesCommand(ILogger<DrillAddOrUpdateResourcesCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<DrillAddOrUpdateResourcesOptions, DrillAddOrUpdateResourcesCommand.DrillAddOrUpdateResourcesCommandResult>
{
    private const int MaxPayloadLength = 1_048_576;
    private readonly ILogger<DrillAddOrUpdateResourcesCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(DrillAddOrUpdateResourcesOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        ValidatePathSegment(options.ServiceGroup, "--service-group", validationResult);
        ValidatePathSegment(options.Drill, "--drill", validationResult);

        if (options.FaultDurationMinutes <= 0)
        {
            validationResult.Errors.Add("--fault-duration-minutes must be greater than zero.");
        }

        try
        {
            _ = CreateContent(options);
        }
        catch (ArgumentException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DrillAddOrUpdateResourcesOptions options, CancellationToken cancellationToken)
    {
        try
        {
            AddOrUpdateResourcesContent content = CreateContent(options);
            DrillAddOrUpdateResourcesResult result = await _resilienceManagementService.AddOrUpdateDrillResourcesAsync(
                options.ServiceGroup,
                options.Drill,
                content,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DrillAddOrUpdateResourcesCommandResult(result),
                ResilienceManagementJsonContext.Default.DrillAddOrUpdateResourcesCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error adding or updating drill resources. ServiceGroup: {ServiceGroup}, Drill: {Drill}.",
                options.ServiceGroup, options.Drill);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal static AddOrUpdateResourcesContent CreateContent(DrillAddOrUpdateResourcesOptions options)
    {
        bool hasInclude = !string.IsNullOrWhiteSpace(options.IncludeResources);
        bool hasUpdate = !string.IsNullOrWhiteSpace(options.UpdateResources);
        bool hasExclude = !string.IsNullOrWhiteSpace(options.ExcludeResources);

        if (!hasInclude && !hasUpdate && !hasExclude)
        {
            throw new ArgumentException("Specify at least one of --include-resources, --update-resources, or --exclude-resources.");
        }

        foreach (string? payload in new[] { options.IncludeResources, options.UpdateResources, options.ExcludeResources })
        {
            if (payload is { } value && Encoding.UTF8.GetByteCount(value) > MaxPayloadLength)
            {
                throw new ArgumentException("Each drill resource JSON payload must not exceed 1 MB.");
            }
        }

        string? forceInclusionAndUpdate = null;
        if (!string.IsNullOrWhiteSpace(options.ForceInclusionAndUpdate))
        {
            if (options.ForceInclusionAndUpdate is not ("Enable" or "Disable"))
            {
                throw new ArgumentException("--force-inclusion-and-update must be Enable or Disable.");
            }

            forceInclusionAndUpdate = options.ForceInclusionAndUpdate;
        }

        try
        {
            using JsonDocument include = JsonDocument.Parse(options.IncludeResources ?? "[]");
            using JsonDocument update = JsonDocument.Parse(options.UpdateResources ?? "[]");
            using JsonDocument exclude = JsonDocument.Parse(options.ExcludeResources ?? "[]");
            ValidateIncludeOrUpdate(include.RootElement, "--include-resources");
            ValidateIncludeOrUpdate(update.RootElement, "--update-resources");
            ValidateExclude(exclude.RootElement);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteNumber("faultDurationInMin", options.FaultDurationMinutes);
                writer.WritePropertyName("resourceLists");
                writer.WriteStartObject();
                writer.WritePropertyName("includeResources");
                include.RootElement.WriteTo(writer);
                writer.WritePropertyName("updateResources");
                update.RootElement.WriteTo(writer);
                writer.WritePropertyName("excludeResources");
                exclude.RootElement.WriteTo(writer);
                writer.WriteEndObject();
                if (forceInclusionAndUpdate is not null)
                {
                    writer.WriteString("forceInclusionAndUpdate", forceInclusionAndUpdate);
                }

                writer.WriteEndObject();
            }

            var reader = new Utf8JsonReader(stream.ToArray());
            var model = new AddOrUpdateResourcesContent(options.FaultDurationMinutes);
            return ((IJsonModel<AddOrUpdateResourcesContent>)model).Create(
                ref reader,
                ModelReaderWriterOptions.Json) ??
                throw new ArgumentException("The drill resource configuration could not be parsed.");
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("Drill resource inputs must be valid JSON.", ex);
        }
    }

    private static void ValidateIncludeOrUpdate(JsonElement resources, string optionName)
    {
        if (resources.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException($"{optionName} must be a JSON array.");
        }

        foreach (JsonElement resource in resources.EnumerateArray())
        {
            if (resource.ValueKind != JsonValueKind.Object ||
                !resource.TryGetProperty("id", out JsonElement idElement) ||
                idElement.ValueKind != JsonValueKind.String ||
                string.IsNullOrWhiteSpace(idElement.GetString()))
            {
                throw new ArgumentException($"Each resource in {optionName} must be an object with a non-empty \"id\" string.");
            }
        }
    }

    private static void ValidateExclude(JsonElement resources)
    {
        if (resources.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException("--exclude-resources must be a JSON array.");
        }

        foreach (JsonElement resource in resources.EnumerateArray())
        {
            if (resource.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(resource.GetString()))
            {
                throw new ArgumentException("Each value in --exclude-resources must be a non-empty Azure resource ID string.");
            }
        }
    }

    private static void ValidatePathSegment(string value, string optionName, ValidationResult validationResult)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Contains('/'))
        {
            validationResult.Errors.Add($"{optionName} must be a single non-empty path segment.");
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
            "Drill resources cannot be added or updated while another drill operation is in progress.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed adding or updating drill resources. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Drill not found. Verify the drill and service group exist and you have access.",
        RequestFailedException =>
            "The drill resource add or update failed. Verify the resource IDs and fault settings, then try again.",
        _ => base.GetErrorMessage(ex)
    };

    public sealed record DrillAddOrUpdateResourcesCommandResult(DrillAddOrUpdateResourcesResult Result);
}
