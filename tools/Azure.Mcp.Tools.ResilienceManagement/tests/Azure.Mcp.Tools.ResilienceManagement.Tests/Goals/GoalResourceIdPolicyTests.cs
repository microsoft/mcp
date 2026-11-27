// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals;

public class GoalResourceIdPolicyTests
{
    private const string GoalResourceId = "/providers/Microsoft.Management/serviceGroups/group/providers/Microsoft.AzureResilienceManagement/goalAssignments/assignment/goalResources/member";
    private const string Body = """{"resources":[{"properties":{"resourceArmId":"/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/group/providers/Microsoft.Storage/storageAccounts/account","zonalResiliency":{"goalParticipation":"Excluded"}}}]}""";

    [Fact]
    public void AddsOnlyDiscoveredId_PreservingNestedGaPayload_OnRepeatedInvocation()
    {
        using var message = CreateMessage();
        var policy = new GoalResourceIdPolicy([GoalResourceId]);
        policy.OnSendingRequest(message);
        policy.OnSendingRequest(message);

        using var stream = new MemoryStream();
        message.Request.Content!.WriteTo(stream, TestContext.Current.CancellationToken);
        using JsonDocument document = JsonDocument.Parse(stream.ToArray());
        JsonElement resource = document.RootElement.GetProperty("resources")[0];
        Assert.Equal(GoalResourceId, resource.GetProperty("id").GetString());
        Assert.Equal("Excluded", resource.GetProperty("properties").GetProperty("zonalResiliency").GetProperty("goalParticipation").GetString());
        Assert.Single(resource.EnumerateObject(), property => property.Name == "id");
        Assert.False(resource.GetProperty("properties").TryGetProperty("highAvailabilityGoalParticipation", out _));
    }

    [Theory]
    [InlineData("GET", "/updateGoalResources")]
    [InlineData("POST", "/recommendCapacity")]
    public void DoesNotChangeOtherRequests(string method, string path)
    {
        using var message = CreateMessage();
        message.Request.Method = new RequestMethod(method);
        message.Request.Uri.Reset(new Uri("https://management.azure.com" + path));
        RequestContent content = message.Request.Content!;
        new GoalResourceIdPolicy([GoalResourceId]).OnSendingRequest(message);
        Assert.Same(content, message.Request.Content);
    }

    [Fact]
    public void RejectsMismatchedIdCount()
    {
        using var message = CreateMessage();
        Assert.Throws<InvalidOperationException>(() => new GoalResourceIdPolicy([]).OnSendingRequest(message));
    }

    [Fact]
    public void RejectsMissingRequestContent()
    {
        using var message = CreateMessage();
        message.Request.Content = null;
        Assert.Throws<InvalidOperationException>(() => new GoalResourceIdPolicy([GoalResourceId]).OnSendingRequest(message));
    }

    private static HttpMessage CreateMessage()
    {
        Request request = HttpClientTransport.Shared.CreateRequest();
        request.Method = RequestMethod.Post;
        request.Uri.Reset(new Uri("https://management.azure.com/goalAssignments/assignment/updateGoalResources"));
        request.Content = RequestContent.Create(BinaryData.FromString(Body));
        return new HttpMessage(request, new ResponseClassifier());
    }
}
