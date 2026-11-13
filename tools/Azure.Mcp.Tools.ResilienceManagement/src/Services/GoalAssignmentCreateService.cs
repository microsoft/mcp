// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
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
        bool requireZonalResiliency,
        bool requireRegionalResiliency,
        string? regionalRecoveryPointObjective,
        string? regionalRecoveryTimeObjective,
        string? tenant = null,
        CancellationToken cancellationToken = default)
    {
        BinaryData payload = CreateGoalAssignmentPayload(
            requireZonalResiliency,
            requireRegionalResiliency,
            regionalRecoveryPointObjective,
            regionalRecoveryTimeObjective);
        var options = ResilienceManagementRoutingPolicy.CreateArmClientOptions();
        options.AddPolicy(
            new GoalAssignmentContractRequestPolicy(payload),
            HttpPipelinePosition.PerCall);
        ArmClient armClient = await CreateArmClientAsync(
            tenantIdOrName: tenant,
            armClientOptions: options,
            cancellationToken: cancellationToken);
        ResourceIdentifier serviceGroupId = new($"/providers/Microsoft.Management/serviceGroups/{serviceGroup}");
        GoalAssignmentCollection goalAssignments = armClient.GetGoalAssignments(serviceGroupId);
        var data = new GoalAssignmentData
        {
            Properties = new GoalAssignmentProperties(requireZonalResiliency)
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

        return JsonSerializer.Deserialize(
            operation.GetRawResponse().Content,
            ResilienceManagementJsonContext.Default.GoalAssignmentInfo)
            ?? throw new InvalidOperationException("The completed goal assignment response was empty.");
    }

    internal static BinaryData CreateGoalAssignmentPayload(
        bool requireZonalResiliency,
        bool requireRegionalResiliency,
        string? regionalRecoveryPointObjective,
        string? regionalRecoveryTimeObjective)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("properties");
            writer.WriteStartObject();
            writer.WriteBoolean("requireZonalResiliency", requireZonalResiliency);
            writer.WriteBoolean("requireRegionalResiliency", requireRegionalResiliency);
            if (requireRegionalResiliency)
            {
                writer.WritePropertyName("regionalObjectives");
                writer.WriteStartObject();
                writer.WriteString(
                    "targetRecoveryPointObjective",
                    regionalRecoveryPointObjective);
                writer.WriteString(
                    "targetRecoveryTimeObjective",
                    regionalRecoveryTimeObjective);
                writer.WriteEndObject();
            }
            writer.WritePropertyName("serviceLevelResources");
            writer.WriteStartArray();
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return BinaryData.FromBytes(stream.ToArray());
    }
}
