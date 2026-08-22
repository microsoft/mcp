// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.Mcp.Tools.ResilienceManagement.Options.Recovery.Plans;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Recovery.Plans;

[CommandMetadata(
    Id = "2fbfa9e6-0a5e-45e4-923d-0ef3706ef733",
    Name = "create",
    Title = "Create or Update Resilience Recovery Plan",
    Description = """
        Creates a new Zonal resilience recovery plan in an Azure service group or updates an existing plan's identity,
        recovery group structure, and recovery group pre/post actions. Use this command to split a plan into additional
        recovery groups or add manual and Azure Automation runbook actions; use recoveryplan resource update instead for
        recovery resource membership and protection settings.
        Creation requires a plan description and a customer-selected SystemAssigned, UserAssigned, or SystemAndUserAssigned
        managed identity. Do not assume an identity type; ask the user to choose one when omitted. Updates can switch identity
        types, but cannot replace an existing user-assigned identity with a different one. Additional recovery groups can be
        replaced with a JSON array containing sequential order IDs, descriptions, optional group GUIDs, and optional pre/post
        actions; they are preserved when omitted. Default group pre/post actions can also be replaced. Actions support manual
        steps and Azure Automation runbook scripts. Updates preserve the default recovery group ID and omitted plan, group,
        and action settings. Plan descriptions must be 5 to 50 characters. A user-assigned identity update must include the
        existing identity's full resource ID; ask for it when omitted.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class RecoveryPlanCreateCommand(ILogger<RecoveryPlanCreateCommand> logger, IResilienceManagementService resilienceManagementService)
    : AuthenticatedCommand<RecoveryPlanCreateOptions, RecoveryPlanCreateCommand.RecoveryPlanCreateCommandResult>
{
    private const int MaxPayloadLength = 1_048_576;
    private readonly ILogger<RecoveryPlanCreateCommand> _logger = logger;
    private readonly IResilienceManagementService _resilienceManagementService = resilienceManagementService;

    public override void ValidateOptions(RecoveryPlanCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.PlanType != Models.RecoveryPlanKind.Zonal)
        {
            validationResult.Errors.Add("Only Zonal recovery plans are currently supported.");
        }

        RecoveryPlanValidation.ValidateServiceGroup(options.ServiceGroup, validationResult);
        RecoveryPlanValidation.ValidateName(options.RecoveryPlan, validationResult);

        if (options.PlanDescription is not null && options.PlanDescription.Length is < 5 or > 50)
        {
            validationResult.Errors.Add("The recovery plan description must be 5 to 50 characters.");
        }

        if (options.DefaultGroupDescription is not null &&
            (string.IsNullOrWhiteSpace(options.DefaultGroupDescription) || options.DefaultGroupDescription.Length is < 5 or > 50))
        {
            validationResult.Errors.Add("The default recovery group description must be 5 to 50 characters and cannot be whitespace when specified.");
        }

        try
        {
            _ = ParseAdditionalGroups(options.AdditionalGroups);
            _ = ParseGroupActions(options.DefaultGroupPreActions, "--default-group-pre-actions");
            _ = ParseGroupActions(options.DefaultGroupPostActions, "--default-group-post-actions");
        }
        catch (ArgumentException ex)
        {
            validationResult.Errors.Add(ex.Message);
        }

        if (options.IdentityType != Models.RecoveryPlanIdentityKind.SystemAssigned && string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
        {
            validationResult.Errors.Add("--user-assigned-identity is required when --identity-type is UserAssigned or SystemAndUserAssigned.");
        }
        else if (options.IdentityType == Models.RecoveryPlanIdentityKind.SystemAssigned && !string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
        {
            validationResult.Errors.Add("--user-assigned-identity is not allowed when --identity-type is SystemAssigned.");
        }
        else if (!string.IsNullOrWhiteSpace(options.UserAssignedIdentity))
        {
            try
            {
                _ = ResilienceManagementService.ParseUserAssignedIdentityResourceId(options.UserAssignedIdentity);
            }
            catch (ArgumentException ex)
            {
                validationResult.Errors.Add(ex.Message);
            }
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, RecoveryPlanCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            IReadOnlyList<RecoveryPlanGroupInput>? additionalGroups = ParseAdditionalGroups(options.AdditionalGroups);
            IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPreActions = ParseGroupActions(options.DefaultGroupPreActions, "--default-group-pre-actions");
            IReadOnlyList<RecoveryPlanGroupActionInput>? defaultGroupPostActions = ParseGroupActions(options.DefaultGroupPostActions, "--default-group-post-actions");
            var recoveryPlan = await _resilienceManagementService.CreateRecoveryPlanAsync(
                options.ServiceGroup,
                options.RecoveryPlan,
                options.PlanType,
                options.PlanDescription,
                options.IdentityType,
                options.UserAssignedIdentity,
                options.DefaultGroupDescription,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken,
                additionalGroups,
                defaultGroupPreActions,
                defaultGroupPostActions);

            context.Response.Results = ResponseResult.Create(
                new RecoveryPlanCreateCommandResult(recoveryPlan),
                ResilienceManagementJsonContext.Default.RecoveryPlanCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating or updating recovery plan. ServiceGroup: {ServiceGroup}, RecoveryPlan: {RecoveryPlan}, PlanType: {PlanType}.",
                options.ServiceGroup, options.RecoveryPlan, options.PlanType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal static IReadOnlyList<RecoveryPlanGroupInput>? ParseAdditionalGroups(string? additionalGroupsJson)
    {
        if (additionalGroupsJson is null)
        {
            return null;
        }

        if (Encoding.UTF8.GetByteCount(additionalGroupsJson) > MaxPayloadLength)
        {
            throw new ArgumentException("The additional recovery groups JSON payload must not exceed 1 MB.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(additionalGroupsJson);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                throw new ArgumentException("--additional-groups must be a JSON array.");
            }

            var groups = new List<RecoveryPlanGroupInput>();
            var groupIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonElement group in document.RootElement.EnumerateArray())
            {
                if (group.ValueKind != JsonValueKind.Object ||
                    !group.TryGetProperty("orderId", out JsonElement orderIdElement) ||
                    !orderIdElement.TryGetInt32(out int orderId) ||
                    !group.TryGetProperty("description", out JsonElement descriptionElement) ||
                    descriptionElement.ValueKind != JsonValueKind.String)
                {
                    throw new ArgumentException("Each additional recovery group must contain an integer orderId and a string description.");
                }

                string description = descriptionElement.GetString()!;
                if (string.IsNullOrWhiteSpace(description) || description.Length is < 5 or > 50)
                {
                    throw new ArgumentException("Each additional recovery group description must contain 5 to 50 characters.");
                }

                if (orderId is < 1 or >= 15)
                {
                    throw new ArgumentException("Each additional recovery group orderId must be between 1 and 14.");
                }

                string? groupUniqueId = null;
                if (group.TryGetProperty("groupUniqueId", out JsonElement groupUniqueIdElement) && groupUniqueIdElement.ValueKind != JsonValueKind.Null)
                {
                    if (groupUniqueIdElement.ValueKind != JsonValueKind.String ||
                        !Guid.TryParse(groupUniqueIdElement.GetString(), out Guid parsedGroupId))
                    {
                        throw new ArgumentException("Each additional recovery group groupUniqueId must be a GUID when specified.");
                    }

                    groupUniqueId = parsedGroupId.ToString();
                    if (!groupIds.Add(groupUniqueId))
                    {
                        throw new ArgumentException("Additional recovery group groupUniqueId values must be unique.");
                    }
                }

                groups.Add(new RecoveryPlanGroupInput(
                    groupUniqueId,
                    orderId,
                    description,
                    ParseGroupActionsProperty(group, "preActions"),
                    ParseGroupActionsProperty(group, "postActions")));
            }

            int expectedOrderId = 1;
            foreach (RecoveryPlanGroupInput group in groups.OrderBy(group => group.OrderId))
            {
                if (group.OrderId != expectedOrderId++)
                {
                    throw new ArgumentException("Additional recovery group orderId values must be unique and sequential starting at 1.");
                }
            }

            return groups;
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("--additional-groups must be a valid JSON array.", ex);
        }
    }

    internal static IReadOnlyList<RecoveryPlanGroupActionInput>? ParseGroupActions(string? actionsJson, string optionName)
    {
        if (actionsJson is null)
        {
            return null;
        }

        if (Encoding.UTF8.GetByteCount(actionsJson) > MaxPayloadLength)
        {
            throw new ArgumentException($"The {optionName} JSON payload must not exceed 1 MB.");
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(actionsJson);
            return ParseGroupActions(document.RootElement, optionName);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"{optionName} must be a valid JSON array.", ex);
        }
    }

    private static IReadOnlyList<RecoveryPlanGroupActionInput>? ParseGroupActionsProperty(JsonElement group, string propertyName)
        => group.TryGetProperty(propertyName, out JsonElement actions) ? ParseGroupActions(actions, propertyName) : null;

    private static IReadOnlyList<RecoveryPlanGroupActionInput> ParseGroupActions(JsonElement actions, string fieldName)
    {
        if (actions.ValueKind != JsonValueKind.Array)
        {
            throw new ArgumentException($"{fieldName} must be a JSON array.");
        }

        var result = new List<RecoveryPlanGroupActionInput>();
        foreach (JsonElement action in actions.EnumerateArray())
        {
            if (action.ValueKind != JsonValueKind.Object ||
                !action.TryGetProperty("type", out JsonElement typeElement) ||
                typeElement.ValueKind != JsonValueKind.String ||
                !TryParseActionType(typeElement.GetString(), out RecoveryPlanGroupActionKind type) ||
                !action.TryGetProperty("name", out JsonElement nameElement) ||
                nameElement.ValueKind != JsonValueKind.String ||
                !IsValidActionName(nameElement.GetString()) ||
                !action.TryGetProperty("timeoutInMinutes", out JsonElement timeoutElement) ||
                !timeoutElement.TryGetInt32(out int timeoutInMinutes) ||
                timeoutInMinutes <= 0)
            {
                throw new ArgumentException($"Each {fieldName} action must contain type (ManualAction or CustomRunbook), a 3 to 24 character name containing only letters, numbers, or hyphens, and a positive integer timeoutInMinutes.");
            }

            string? description = null;
            if (action.TryGetProperty("description", out JsonElement descriptionElement) && descriptionElement.ValueKind != JsonValueKind.Null)
            {
                if (descriptionElement.ValueKind != JsonValueKind.String)
                {
                    throw new ArgumentException($"Each {fieldName} action description must be a string when specified.");
                }

                description = descriptionElement.GetString();
                if (description!.Length > 100)
                {
                    throw new ArgumentException($"Each {fieldName} action description must not exceed 100 characters.");
                }
            }

            string? actionResourceId = null;
            IReadOnlyDictionary<string, string>? parameters = null;
            if (type == RecoveryPlanGroupActionKind.CustomRunbook)
            {
                if (!action.TryGetProperty("actionResourceId", out JsonElement actionResourceIdElement) ||
                    actionResourceIdElement.ValueKind != JsonValueKind.String ||
                    string.IsNullOrWhiteSpace(actionResourceIdElement.GetString()))
                {
                    throw new ArgumentException($"Each CustomRunbook {fieldName} action requires actionResourceId.");
                }

                actionResourceId = actionResourceIdElement.GetString()!;
                ValidateRunbookResourceId(actionResourceId, fieldName);
                parameters = ParseActionParameters(action, fieldName);
            }

            result.Add(new RecoveryPlanGroupActionInput(
                type,
                nameElement.GetString()!,
                description,
                timeoutInMinutes,
                actionResourceId,
                parameters));
        }

        return result;
    }

    private static bool IsValidActionName(string? name)
        => name is { Length: >= 3 and <= 24 } && name.All(character =>
            character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-');

    private static bool TryParseActionType(string? value, out RecoveryPlanGroupActionKind type)
    {
        if (string.Equals(value, nameof(RecoveryPlanGroupActionKind.ManualAction), StringComparison.OrdinalIgnoreCase))
        {
            type = RecoveryPlanGroupActionKind.ManualAction;
            return true;
        }

        if (string.Equals(value, nameof(RecoveryPlanGroupActionKind.CustomRunbook), StringComparison.OrdinalIgnoreCase))
        {
            type = RecoveryPlanGroupActionKind.CustomRunbook;
            return true;
        }

        type = default;
        return false;
    }

    private static IReadOnlyDictionary<string, string>? ParseActionParameters(JsonElement action, string fieldName)
    {
        if (!action.TryGetProperty("parameters", out JsonElement parametersElement))
        {
            return null;
        }

        if (parametersElement.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (parametersElement.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException($"Each CustomRunbook {fieldName} action parameters value must be a JSON object of string values.");
        }

        var parameters = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (JsonProperty parameter in parametersElement.EnumerateObject())
        {
            if (parameter.Value.ValueKind != JsonValueKind.String)
            {
                throw new ArgumentException($"Each CustomRunbook {fieldName} action parameter value must be a string.");
            }

            parameters.Add(parameter.Name, parameter.Value.GetString()!);
        }

        return parameters;
    }

    private static void ValidateRunbookResourceId(string actionResourceId, string fieldName)
    {
        try
        {
            var resourceId = new ResourceIdentifier(actionResourceId);
            if (!string.Equals(resourceId.ResourceType.ToString(), "Microsoft.Automation/automationAccounts/runbooks", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Each CustomRunbook {fieldName} actionResourceId must identify a Microsoft.Automation/automationAccounts/runbooks resource.");
            }
        }
        catch (ArgumentException ex) when (!ex.Message.Contains("must identify", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Each CustomRunbook {fieldName} actionResourceId must be a valid Azure resource ID.", ex);
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
            "The recovery plan could not be created or updated because it conflicts with the current resource state.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            "Authorization failed creating or updating the recovery plan. Verify you have the required permissions.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Service group not found. Verify the service group exists and you have access.",
        RequestFailedException =>
            "The recovery plan request failed. Verify the request parameters and try again.",
        _ => base.GetErrorMessage(ex)
    };

    public record RecoveryPlanCreateCommandResult(Models.RecoveryPlanInfo RecoveryPlan);
}
