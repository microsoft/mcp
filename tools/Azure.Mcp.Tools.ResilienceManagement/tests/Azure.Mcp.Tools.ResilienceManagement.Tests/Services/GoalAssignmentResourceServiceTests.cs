// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.ClientModel.Primitives;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Azure.Mcp.Tools.ResilienceManagement.Tests.Goals.Assignments;
using Azure.ResourceManager.ResilienceManagement;
using Azure.ResourceManager.ResilienceManagement.Models;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Services;

public sealed class GoalAssignmentResourceServiceTests
{
    [Theory]
    [InlineData("operation-id", "11111111-1111-1111-1111-111111111111")]
    [InlineData("Azure-AsyncOperation", "https://management.azure.com/operationStatuses/11111111-1111-1111-1111-111111111111?t=secret")]
    [InlineData("Location", "https://management.azure.com/operationResults/11111111-1111-1111-1111-111111111111?t=secret&c=secret")]
    [InlineData("Azure-AsyncOperation", "https://management.azure.com/operationStatuses/11111111-1111-1111-1111-111111111111*ABC123?t=secret&c=secret")]
    public void MapsActualOperationIdWithoutSignedQuery(string header, string value)
    {
        using var response = new GoalAssignmentTestResponse(202, header, value);
        var result = ResilienceManagementService.MapGoalAssignmentOperation(response, true);
        Assert.Equal("11111111-1111-1111-1111-111111111111", result.OperationId);
        Assert.Equal("Accepted", result.Status);
        Assert.False(result.HasCompleted);
    }

    [Fact]
    public void DoesNotInventOperationId()
    {
        using var response = new GoalAssignmentTestResponse(202, "Location", "https://management.azure.com/resource?secret=secret");
        Assert.Null(ResilienceManagementService.MapGoalAssignmentOperation(response, false).OperationId);
    }

    [Fact]
    public void MapsSynchronousCompletion()
    {
        using var response = new GoalAssignmentTestResponse(200, "none", "");
        var result = ResilienceManagementService.MapGoalAssignmentOperation(response, true);
        Assert.Equal("Completed", result.Status);
        Assert.True(result.HasCompleted);
    }

    [Fact]
    public void InstalledSdkWireFormatDropsRequiredResourceId()
    {
        var payload = BinaryData.FromString($$"""{"resources":{{GoalAssignmentUpdateResourcesCommandTests.Resources}}}""");
        var content = ModelReaderWriter.Read<UpdateGoalResourceContent>(payload, ModelReaderWriterOptions.Json, AzureResourceManagerResilienceManagementContext.Default)!;
        Assert.NotNull(content.Resources[0].Id);
        var wire = ModelReaderWriter.Write(content, new ModelReaderWriterOptions("W"), AzureResourceManagerResilienceManagementContext.Default);
        using var document = JsonDocument.Parse(wire);
        Assert.False(document.RootElement.GetProperty("resources")[0].TryGetProperty("id", out _));
    }

    [Theory]
    [InlineData("POST", "/updateGoalResources", true)]
    [InlineData("GET", "/updateGoalResources", false)]
    [InlineData("POST", "/recommendCapacity", false)]
    public void PayloadPolicyOnlyChangesResourceUpdatePost(string method, string path, bool changes)
    {
        using var transport = new HttpClientTransport();
        using var request = transport.CreateRequest();
        request.Method = new RequestMethod(method);
        request.Uri.Reset(new Uri("https://management.azure.com" + path));
        request.Content = RequestContent.Create(BinaryData.FromString("{}"));
        using var message = new HttpMessage(request, new ResponseClassifier());
        var payload = BinaryData.FromString($$"""{"resources":{{GoalAssignmentUpdateResourcesCommandTests.Resources}}}""");
        new GoalResourceUpdateRequestPolicy(payload).OnSendingRequest(message);
        using var stream = new MemoryStream();
        request.Content.WriteTo(stream, TestContext.Current.CancellationToken);
        Assert.Equal(changes ? payload.ToString() : "{}", System.Text.Encoding.UTF8.GetString(stream.ToArray()));
    }
}
