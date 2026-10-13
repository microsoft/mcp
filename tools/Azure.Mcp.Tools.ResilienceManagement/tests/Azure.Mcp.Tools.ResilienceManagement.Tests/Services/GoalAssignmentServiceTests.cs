// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.ResourceManager;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class GoalAssignmentServiceTests
{
    private const string SubscriptionId = "11111111-1111-1111-1111-111111111111";
    private const string OperationId = "22222222-2222-2222-2222-222222222222";
    private const string ServiceLevelResourceId = "/providers/Microsoft.Management/serviceGroups/sg1/providers/Microsoft.Monitor/slis/sli1";
    private const string OperationUrl = "https://management.azure.com/subscriptions/" + SubscriptionId +
        "/providers/Microsoft.AzureResilienceManagement/locations/eastus/operationStatuses/" + OperationId + "?api-version=2026-10-01";

    [Theory]
    [InlineData("*C3775EF2AC0E1E4AA241CB87BEE5AA96EBB7B61E6391AE2CEABF0BAB8702AA1E", true)]
    [InlineData("%2AC3775EF2AC0E1E4AA241CB87BEE5AA96EBB7B61E6391AE2CEABF0BAB8702AA1E", true)]
    [InlineData("*invalid", false)]
    [InlineData("extra", false)]
    public void PollingUrl_ExtractsExactGuidBeforeRoutingHash(string suffix, bool valid)
    {
        const string id = "10b982d0-4c82-484a-942b-cf694cdcac58";
        string? result = ResilienceManagementService.ExtractGoalOperationId(
            $"https://management.azure.com/providers/Microsoft.AzureResilienceManagement/locations/CENTRALUS/operationStatuses/{id}{suffix}?api-version=2026-10-01");

        Assert.Equal(valid ? id : null, result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateGoalAssignmentData_UsesExactGaWireContract(bool required)
    {
        var data = ResilienceManagementService.CreateGoalAssignmentData(required, [ServiceLevelResourceId, ServiceLevelResourceId + "2"]);

        using var actual = JsonDocument.Parse(ModelReaderWriter.Write(data, new ModelReaderWriterOptions("W")).ToString());
        using var expected = JsonDocument.Parse($$"""
            {"properties":{"requireZonalResiliency":{{(required ? "true" : "false")}},"serviceLevelResources":[
                {"serviceLevelIndicatorResourceId":"{{ServiceLevelResourceId}}"},
                {"serviceLevelIndicatorResourceId":"{{ServiceLevelResourceId}}2"}] } }
            """);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement), actual.RootElement.GetRawText());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateGoalAssignmentData_ExplicitClearSerializesEmptyArrayAndPreservesZonalRequirement(bool required)
    {
        var data = ResilienceManagementService.CreateGoalAssignmentData(required, []);

        using var document = JsonDocument.Parse(ModelReaderWriter.Write(data, new ModelReaderWriterOptions("W")).ToString());
        using var expected = JsonDocument.Parse($$"""
            {"properties":{"requireZonalResiliency":{{(required ? "true" : "false")}},"serviceLevelResources":[]} }
            """);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, document.RootElement), document.RootElement.GetRawText());
    }

    [Theory]
    [InlineData(OperationId)]
    [InlineData(OperationUrl)]
    [InlineData("https://management.azure.com/providers/Microsoft.AzureResilienceManagement/operationResults/" + OperationId)]
    [InlineData("https://management.azure.com/providers/Microsoft.AzureResilienceManagement/operations/" + OperationId)]
    [InlineData("https://management.azure.com/providers/Microsoft.AzureResilienceManagement/OPERATIONSTATUSES/" + OperationId)]
    public void ExtractGoalOperationId_ExtractsOnlyActualOperationGuid(string value) =>
        Assert.Equal(OperationId, ResilienceManagementService.ExtractGoalOperationId(value));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-id")]
    [InlineData("22222222222222222222222222222222")]
    [InlineData("https://management.azure.com/subscriptions/" + SubscriptionId)]
    [InlineData("https://management.azure.com/subscriptions/" + SubscriptionId + "/providers/Microsoft.AzureResilienceManagement/operationStatuses/not-an-operation")]
    [InlineData("https://management.azure.com/subscriptions/" + SubscriptionId + "?operationId=" + OperationId)]
    [InlineData("https://management.azure.com/resource/" + OperationId)]
    public void ExtractGoalOperationId_DoesNotConfuseSubscriptionOrUnrelatedGuidWithOperation(string? value) =>
        Assert.Null(ResilienceManagementService.ExtractGoalOperationId(value));

    [Theory]
    [InlineData("Azure-AsyncOperation")]
    [InlineData("Operation-Location")]
    [InlineData("Location")]
    public void CreateGoalAssignmentOperationResult_UsesServiceHeaderAndReportsAccepted(string header)
    {
        var operation = CreateOperation(false, "not-a-service-operation-id", new() { [header] = OperationUrl });

        var result = ResilienceManagementService.CreateGoalAssignmentOperationResult("sg1", "assignment1", operation);

        Assert.Equal("sg1", result.ServiceGroup);
        Assert.Equal("assignment1", result.GoalAssignment);
        Assert.Equal(OperationId, result.OperationId);
        Assert.Equal("Accepted", result.Status);
        Assert.False(result.HasCompleted);
    }

    [Fact]
    public void CreateGoalAssignmentOperationResult_FallsBackPastInvalidHeaderToOperationId()
    {
        var operation = CreateOperation(false, OperationId, new()
        {
            ["Azure-AsyncOperation"] = "https://management.azure.com/subscriptions/" + SubscriptionId,
            ["Location"] = "unrecognized"
        });

        var result = ResilienceManagementService.CreateGoalAssignmentOperationResult("sg1", "assignment1", operation);

        Assert.Equal(OperationId, result.OperationId);
        Assert.False(result.HasCompleted);
    }

    [Fact]
    public void CreateGoalAssignmentOperationResult_PrefersPollingHeaderOverFallbackId()
    {
        var operation = CreateOperation(false, SubscriptionId, new()
        {
            ["Azure-AsyncOperation"] = OperationUrl,
            ["Location"] = "https://management.azure.com/operations/" + SubscriptionId
        });

        Assert.Equal(OperationId, ResilienceManagementService.CreateGoalAssignmentOperationResult("sg1", "assignment1", operation).OperationId);
    }

    [Fact]
    public void CreateGoalAssignmentOperationResult_RejectsAcceptedOperationWithoutRecognizableId()
    {
        var operation = CreateOperation(false, "NOT_SET", []);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            ResilienceManagementService.CreateGoalAssignmentOperationResult("sg1", "assignment1", operation));

        Assert.Contains("Completion is unknown", exception.Message);
    }

    [Theory]
    [InlineData(OperationId, OperationId)]
    [InlineData("NOT_SET", null)]
    public void CreateGoalAssignmentOperationResult_ReportsCompletedWithoutFabricatingId(string id, string? expectedId)
    {
        var operation = CreateOperation(true, id, []);

        var result = ResilienceManagementService.CreateGoalAssignmentOperationResult("sg1", "assignment1", operation);

        Assert.True(result.HasCompleted);
        Assert.Equal("Succeeded", result.Status);
        Assert.Equal(expectedId, result.OperationId);
    }

    private static ArmOperation CreateOperation(bool completed, string id, Dictionary<string, string> headers)
    {
        var operation = Substitute.For<ArmOperation>();
        operation.Id.Returns(id);
        operation.HasCompleted.Returns(completed);
        operation.GetRawResponse().Returns(new GoalAssignmentTestResponse(completed ? 200 : 202, headers));
        return operation;
    }
}
