// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager.ResilienceManagement;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResilienceManagement.Commands.Goals.Assignments;

internal static class GoalAssignmentResourceValidation
{
    internal static void ValidateNames(string serviceGroup, string goalAssignment, ValidationResult result)
    {
        ValidateName(serviceGroup, "--service-group", result);
        ValidateName(goalAssignment, "--goal-assignment", result);
    }

    private static void ValidateName(string value, string option, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value) || value is "." or ".." ||
            value.Any(c => char.IsControl(c) || c is '/' or '\\' or '?' or '#' or '%'))
        {
            result.Errors.Add($"{option} must be a single non-empty ARM path segment.");
        }
    }

    internal static bool IsAzureResourceId(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Any(c => char.IsControl(c) || c is '?' or '#' or '%' or '\\') ||
            value.Split('/').Any(segment => segment is "." or ".." || string.IsNullOrWhiteSpace(segment) && segment != string.Empty) ||
            value.Contains("//", StringComparison.Ordinal) || value.EndsWith('/'))
        {
            return false;
        }

        return ResourceIdentifier.TryParse(value, out var id) && id is not null && Guid.TryParse(id.SubscriptionId, out _) &&
            !string.IsNullOrEmpty(id.ResourceGroupName) && id.ResourceType.Namespace != "Microsoft.Resources" &&
            value.StartsWith("/subscriptions/", StringComparison.OrdinalIgnoreCase);
    }

    internal static void ValidateResourceIds(string[]? ids, ValidationResult result)
    {
        if (ids is not null && (ids.Length == 0 || ids.Any(id => !IsAzureResourceId(id)) ||
            ids.Distinct(StringComparer.OrdinalIgnoreCase).Count() != ids.Length))
        {
            result.Errors.Add("--resource-ids must contain unique, complete Azure resource ARM IDs; omit it to assess all eligible resources.");
        }
    }

    internal static void ValidateResources(string resources, string serviceGroup, string assignment, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(resources) || Encoding.UTF8.GetByteCount(resources) > 1_048_576)
        {
            result.Errors.Add("--resources must be a non-empty JSON array no larger than 1 MiB.");
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(resources, new JsonDocumentOptions { MaxDepth = 16 });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
            {
                throw new JsonException();
            }

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var armIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var resource in root.EnumerateArray())
            {
                CheckFields(resource, ["id", "properties"]);
                var id = RequiredString(resource, "id");
                if (!ResourceIdentifier.TryParse(id, out var parsed) || parsed is null || !Guid.TryParse(parsed.Name, out _) ||
                    !string.Equals(id, GoalMembersResource.CreateResourceIdentifier(serviceGroup, assignment, parsed.Name).ToString(), StringComparison.OrdinalIgnoreCase) ||
                    !ids.Add(id))
                {
                    throw new JsonException();
                }

                var properties = resource.GetProperty("properties");
                CheckFields(properties, ["resourceArmId", "highAvailabilityGoalParticipation", "highAvailabilityAttestationStatus",
                    "disasterRecoveryGoalParticipation", "disasterRecoveryAttestationStatus",
                    "userConfirmationForHighAvailability"]);
                var armId = RequiredString(properties, "resourceArmId");
                if (!IsAzureResourceId(armId) || !armIds.Add(armId))
                {
                    throw new JsonException();
                }

                CheckEnum(properties, "highAvailabilityGoalParticipation", ["Included", "Excluded"], required: true);
                CheckEnum(properties, "highAvailabilityAttestationStatus", ["NotAttested", "ManuallyAttested"], required: true);
                CheckEnum(properties, "disasterRecoveryGoalParticipation", ["Included", "Excluded"]);
                CheckEnum(properties, "disasterRecoveryAttestationStatus", ["NotAttested", "ManuallyAttested"]);
                if (properties.TryGetProperty("userConfirmationForHighAvailability", out var confirmations))
                {
                    if (confirmations.ValueKind != JsonValueKind.Array)
                    {
                        throw new JsonException();
                    }
                    foreach (var confirmation in confirmations.EnumerateArray())
                    {
                        CheckFields(confirmation, ["solutionDisplayName", "confirmationStatus", "reasonForRequestingConfirmation"]);
                        CheckEnum(confirmation, "solutionDisplayName", ["ZonePinnedVmWithZrsDisk", "VmInMultiZoneVmss"], required: true);
                        CheckEnum(confirmation, "confirmationStatus", ["ApprovedByUser", "ApprovalPending", "ApprovalNotNeeded", "RejectedByUser"], required: true);
                        CheckEnum(confirmation, "reasonForRequestingConfirmation", ["ZonePinnedZrsDataDisksConditional", "VmInMultiZoneScaleSetStatelessOnly"]);
                    }
                }
            }
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
        {
            result.Errors.Add("--resources must contain unique goal resource IDs belonging to this assignment, valid resourceArmId values, and properties matching the documented goal resource update schema.");
        }
    }

    private static string RequiredString(JsonElement value, string name) =>
        value.GetProperty(name).ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetProperty(name).GetString())
            ? value.GetProperty(name).GetString()! : throw new JsonException();

    private static void CheckFields(JsonElement value, string[] allowed)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException();
        }
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in value.EnumerateObject())
        {
            if (!allowed.Contains(property.Name, StringComparer.Ordinal) || !names.Add(property.Name))
            {
                throw new JsonException();
            }
        }
    }

    private static void CheckEnum(JsonElement value, string name, string[] allowed, bool required = false)
    {
        if ((required || value.TryGetProperty(name, out _)) &&
            !allowed.Contains(RequiredString(value, name), StringComparer.Ordinal))
        {
            throw new JsonException();
        }
    }

    internal static string GetErrorMessage(Exception exception) => exception switch
    {
        RequestFailedException { Status: 404 } => "Goal assignment or resource not found. Verify the service group, assignment, and resource IDs.",
        RequestFailedException { Status: 403 } => "Authorization failed. Verify access to the goal assignment and its service group in the selected tenant.",
        RequestFailedException { Status: 409 } => "The goal assignment has a conflicting operation or state. Inspect its state before retrying.",
        OperationCanceledException => "The goal assignment operation was canceled. Completion is unknown; inspect resource state before retrying.",
        _ => "The goal assignment resource operation failed. Verify the assignment state and input, then try again."
    };
}
