// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed partial class ResilienceManagementService
{
    public async Task<GoalAssignmentInfo> UpdateGoalAssignmentAsync(
        string serviceGroup,
        string goalAssignment,
        IReadOnlyList<string> serviceLevelResourceIds,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateArmClientAsync(
            tenantIdOrName: tenant,
            armClientOptions: CreateGoalAssignmentContractArmClientOptions(),
            cancellationToken: cancellationToken);
        GoalAssignmentResource resource = client.GetGoalAssignmentResource(
            GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment));
        Response<GoalAssignmentResource> current = await resource.GetAsync(cancellationToken);
        using JsonDocument document = JsonDocument.Parse(current.GetRawResponse().Content);
        bool requireZonalResiliency =
            GetOptionalBoolean(GetOptionalObject(document.RootElement, "properties"), "requireZonalResiliency") ??
            throw new InvalidOperationException("The existing assignment has no GA zonal requirement to preserve.");
        GoalAssignmentData data = CreateGoalAssignmentData(requireZonalResiliency, serviceLevelResourceIds);
        ArmOperation operation = await resource.UpdateAsync(WaitUntil.Started, data, cancellationToken);
        await WaitForLroCompletionAsync(operation, cancellationToken);
        Response<GoalAssignmentResource> updated = await resource.GetAsync(cancellationToken);
        return JsonSerializer.Deserialize(
            updated.GetRawResponse().Content,
            Commands.ResilienceManagementJsonContext.Default.GoalAssignmentInfo)
            ?? throw new InvalidOperationException("The updated goal assignment response was empty.");
    }

    internal static GoalAssignmentData CreateGoalAssignmentData(
        bool requireZonalResiliency,
        IReadOnlyList<string> serviceLevelResourceIds)
    {
        var properties = new GoalAssignmentProperties(requireZonalResiliency);
        properties.ServiceLevelResources.Clear();
        foreach (string id in serviceLevelResourceIds)
        {
            properties.ServiceLevelResources.Add(new ServiceLevelTarget(new ResourceIdentifier(id)));
        }

        return new GoalAssignmentData { Properties = properties };
    }

    internal static GoalAssignmentOperationResult CreateGoalAssignmentOperationResult(
        string serviceGroup,
        string goalAssignment,
        ArmOperation operation)
    {
        Response response = operation.GetRawResponse();
        string? operationId = null;
        foreach (string header in new[] { "Azure-AsyncOperation", "Operation-Location", "Location" })
        {
            if (response.Headers.TryGetValue(header, out string? location))
            {
                operationId = ExtractGoalOperationId(location);
                if (operationId is not null)
                {
                    break;
                }
            }
        }

        operationId ??= ExtractGoalOperationId(operation.Id);
        if (operationId is null && !operation.HasCompleted)
        {
            throw new InvalidOperationException(
                "The service accepted the request without a recognizable operation ID. Completion is unknown.");
        }

        return new GoalAssignmentOperationResult(
            serviceGroup,
            goalAssignment,
            operationId,
            operation.HasCompleted ? "Succeeded" : "Accepted",
            operation.HasCompleted);
    }

    internal static string? ExtractGoalOperationId(string? value)
    {
        if (Guid.TryParseExact(value, "D", out Guid directId))
        {
            return directId.ToString();
        }

        if (Uri.TryCreate(value, UriKind.Absolute, out Uri? uri))
        {
            string[] segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i + 1 < segments.Length; i++)
            {
                if ((segments[i].Equals("operationStatuses", StringComparison.OrdinalIgnoreCase) ||
                     segments[i].Equals("operationResults", StringComparison.OrdinalIgnoreCase) ||
                     segments[i].Equals("operations", StringComparison.OrdinalIgnoreCase)) &&
                    TryParseGoalPollingId(Uri.UnescapeDataString(segments[i + 1]), out Guid id))
                {
                    return id.ToString();
                }
            }
        }

        return null;
    }

    private static bool TryParseGoalPollingId(string segment, out Guid id)
    {
        int separator = segment.IndexOf('*');
        if (separator >= 0)
        {
            string hash = segment[(separator + 1)..];
            if (hash.Length != 64 || !hash.All(char.IsAsciiHexDigit))
            {
                id = default;
                return false;
            }

            segment = segment[..separator];
        }

        return Guid.TryParseExact(segment, "D", out id);
    }
}
