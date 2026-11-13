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
    private static readonly TimeSpan GoalAssignmentOperationTimeout = TimeSpan.FromMinutes(10);

    private Task<ArmClient> CreateGoalArmClientAsync(
        string? tenantIdOrName, CancellationToken cancellationToken, ArmClientOptions? armClientOptions = null)
    {
#if DEBUG
        if (Environment.GetEnvironmentVariable("MCP_GOALS_TEST_ECY") == "true")
        {
            if (!Uri.TryCreate(Environment.GetEnvironmentVariable("TEST_PROXY_URL"), UriKind.Absolute, out _))
            {
                throw new InvalidOperationException("Goals canary tests require TEST_PROXY_URL to capture every request.");
            }

            armClientOptions ??= new ArmClientOptions();
            armClientOptions.AddPolicy(new GoalTestRoutingPolicy(), HttpPipelinePosition.PerCall);
        }
#endif
        return CreateArmClientAsync(tenantIdOrName, armClientOptions, cancellationToken);
    }

    public async Task<GoalAssignmentInfo> CreateGoalAssignmentAsync(
        string serviceGroup, string goalAssignment, bool requireZonalResiliency,
        IReadOnlyList<string>? serviceLevelResourceIds = null, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        ResourceIdentifier id = GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment);
        GoalAssignmentCollection assignments = client.GetGoalAssignments(id.Parent);
        GoalAssignmentData data = CreateGoalAssignmentData(requireZonalResiliency, serviceLevelResourceIds ?? []);
        ArmOperation operation = await assignments.CreateOrUpdateAsync(WaitUntil.Started, goalAssignment, data, cancellationToken);
        await WaitForGoalAssignmentCompletionAsync(operation, cancellationToken);
        Response<GoalAssignmentResource> response = await assignments.GetAsync(goalAssignment, cancellationToken);
        return MapGoalAssignment(response.Value.Data, response.GetRawResponse().Content);
    }

    public async Task<GoalAssignmentInfo> UpdateGoalAssignmentAsync(
        string serviceGroup, string goalAssignment, IReadOnlyList<string> serviceLevelResourceIds,
        string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        GoalAssignmentResource resource = client.GetGoalAssignmentResource(
            GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment));
        Response<GoalAssignmentResource> current = await resource.GetAsync(cancellationToken);
        using JsonDocument document = JsonDocument.Parse(current.GetRawResponse().Content);
        bool requireZonalResiliency = GetOptionalBoolean(GetOptionalObject(document.RootElement, "properties"), "requireZonalResiliency") ??
            throw new InvalidOperationException("The existing assignment has no GA zonal requirement to preserve.");
        GoalAssignmentData data = CreateGoalAssignmentData(requireZonalResiliency, serviceLevelResourceIds);
        ArmOperation operation = await resource.UpdateAsync(WaitUntil.Started, data, cancellationToken);
        await WaitForGoalAssignmentCompletionAsync(operation, cancellationToken);
        Response<GoalAssignmentResource> updated = await resource.GetAsync(cancellationToken);
        return MapGoalAssignment(updated.Value.Data, updated.GetRawResponse().Content);
    }

    internal static GoalAssignmentData CreateGoalAssignmentData(bool requireZonalResiliency, IReadOnlyList<string> serviceLevelResourceIds)
    {
        var properties = new GoalAssignmentProperties(requireZonalResiliency);
        properties.ServiceLevelResources.Clear();
        foreach (string id in serviceLevelResourceIds)
        {
            properties.ServiceLevelResources.Add(new ServiceLevelTarget(new ResourceIdentifier(id)));
        }

        return new GoalAssignmentData { Properties = properties };
    }

    public async Task<bool> DeleteGoalAssignmentAsync(
        string serviceGroup, string goalAssignment, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        GoalAssignmentResource resource = client.GetGoalAssignmentResource(
            GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment));
        try
        {
            _ = await resource.GetAsync(cancellationToken);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return false;
        }

        ArmOperation operation = await resource.DeleteAsync(WaitUntil.Started, cancellationToken);
        await WaitForGoalAssignmentCompletionAsync(operation, cancellationToken);
        return true;
    }

    public async Task<GoalAssignmentOperationResult> RecommendGoalAssignmentCapacityAsync(
        string serviceGroup, string goalAssignment, IReadOnlyList<string> resourceIds,
        string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        GoalAssignmentResource resource = client.GetGoalAssignmentResource(
            GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment));
        var content = new RecommendCapacityContent(resourceIds.Select(id => new ResourceIdentifier(id)));
        ArmOperation operation = await resource.RecommendCapacityAsync(WaitUntil.Started, content, cancellationToken);
        return CreateGoalAssignmentOperationResult(serviceGroup, goalAssignment, operation);
    }

    public async Task<GoalAssignmentOperationResult> RefreshGoalAssignmentResourcesAsync(
        string serviceGroup, string goalAssignment, string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        GoalAssignmentResource resource = client.GetGoalAssignmentResource(
            GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment));
        ArmOperation operation = await resource.RefreshGoalResourcesAsync(WaitUntil.Started, cancellationToken);
        return CreateGoalAssignmentOperationResult(serviceGroup, goalAssignment, operation);
    }

    public async Task<GoalAssignmentOperationResult> UpdateGoalAssignmentResourcesAsync(
        string serviceGroup, string goalAssignment, UpdateGoalResourceContent content,
        string? tenant = null, CancellationToken cancellationToken = default)
    {
        ArmClient client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        GoalAssignmentResource resource = client.GetGoalAssignmentResource(
            GoalAssignmentResource.CreateResourceIdentifier(serviceGroup, goalAssignment));
        var members = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await foreach (GoalMembersResource member in resource.GetAllGoalMembers().GetAllAsync(cancellationToken: cancellationToken))
        {
            members.Add(member.Data.Properties.ResourceArmId.ToString(), member.Id.ToString());
        }

        string[] resourceIds = content.Resources.Select(member =>
            members.TryGetValue(member.Properties.ResourceArmId.ToString(), out string? id)
                ? id
                : throw new KeyNotFoundException("A requested resource is not a member of the selected goal assignment. Refresh resources and try again."))
            .ToArray();
        var options = new ArmClientOptions();
        options.AddPolicy(new GoalResourceIdPolicy(resourceIds), HttpPipelinePosition.PerCall);
        client = await CreateGoalArmClientAsync(tenantIdOrName: tenant, armClientOptions: options, cancellationToken: cancellationToken);
        resource = client.GetGoalAssignmentResource(resource.Id);
        ArmOperation operation = await resource.UpdateGoalResourcesAsync(WaitUntil.Started, content, cancellationToken);
        return CreateGoalAssignmentOperationResult(serviceGroup, goalAssignment, operation);
    }

    private static async Task WaitForGoalAssignmentCompletionAsync(Operation operation, CancellationToken cancellationToken)
    {
        await ExecuteWithTimeoutAsync(
            async token =>
            {
                await WaitForLroCompletionAsync(operation, token);
                return true;
            },
            "goal assignment operation", GoalAssignmentOperationTimeout, cancellationToken);
    }

    internal static GoalAssignmentOperationResult CreateGoalAssignmentOperationResult(
        string serviceGroup, string goalAssignment, ArmOperation operation)
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
            throw new InvalidOperationException("The service accepted the request without a recognizable operation ID. Completion is unknown.");
        }

        return new GoalAssignmentOperationResult(serviceGroup, goalAssignment, operationId,
            operation.HasCompleted ? "Succeeded" : "Accepted", operation.HasCompleted);
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
        // ARM appends a routing hash to the operation GUID in Goals polling URLs.
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
