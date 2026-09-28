// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.ResilienceManagement.Commands;
using Azure.Mcp.Tools.ResilienceManagement.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;

namespace Azure.Mcp.Tools.ResilienceManagement.Services;

public sealed class GoalAssignmentCreateService(IAzureService azureService)
    : BaseAzureResourceService(azureService), IGoalAssignmentCreateService
{
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromMinutes(10);

    public async Task<GoalAssignmentInfo> CreateGoalAssignmentAsync(
        string serviceGroup,
        string goalAssignment,
        string goalTemplate,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        ArmClient armClient = await CreateArmClientAsync(tenantIdOrName: tenant, cancellationToken: cancellationToken);
        ResourceIdentifier serviceGroupId = new($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalAssignmentCollection goalAssignments = armClient.GetGoalAssignments(serviceGroupId);
        ResourceIdentifier goalTemplateId = GoalTemplateResource.CreateResourceIdentifier(serviceGroup, goalTemplate);
        var data = new GoalAssignmentData
        {
            Properties = new GoalAssignmentProperties(goalTemplateId, GoalAssignmentType.Resiliency)
        };

        ArmOperation operation = await goalAssignments.CreateOrUpdateAsync(WaitUntil.Started, goalAssignment, data, cancellationToken);
        using var timeoutCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCancellation.CancelAfter(OperationTimeout);

        try
        {
            await WaitForLroCompletionAsync(operation, timeoutCancellation.Token);
        }
        catch (OperationCanceledException) when (timeoutCancellation.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("The goal assignment create or update did not complete within 10 minutes.");
        }

        return operation.GetRawResponse().Content.ToObjectFromJson(
            ResilienceManagementJsonContext.Default.GoalAssignmentInfo)
            ?? throw new InvalidOperationException("The completed goal assignment response was empty.");
    }
}
