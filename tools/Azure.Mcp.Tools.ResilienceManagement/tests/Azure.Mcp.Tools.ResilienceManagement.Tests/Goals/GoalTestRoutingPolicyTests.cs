// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#if DEBUG
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Tools.ResilienceManagement.Services;
using Xunit;

namespace Azure.Mcp.Tools.ResilienceManagement.Tests.Goals;

public class GoalTestRoutingPolicyTests
{
    [Theory]
    [InlineData("management.azure.com")]
    [InlineData("eastus2euap.management.azure.com")]
    public void RoutesRequestAndPollingWithoutChangingPayloadOrProxyHeaders(string host)
    {
        using Request request = HttpClientTransport.Shared.CreateRequest();
        request.Uri.Reset(new Uri($"https://{host}/providers/Microsoft.AzureResilienceManagement/locations/EASTUS2EUAP/operationStatuses/operation?api-version=2026-10-01"));
        request.Content = RequestContent.Create("""{"properties":{"serviceLevelResources":[]}}""");
        request.Headers.SetValue("x-recording-id", "recording");
        using var message = new HttpMessage(request, new ResponseClassifier());
        RequestContent content = request.Content;
        var policy = new GoalTestRoutingPolicy();

        policy.OnSendingRequest(message);
        policy.OnSendingRequest(message);

        Assert.Equal("eastus2euap.management.azure.com", request.Uri.Host);
        Assert.Equal("?api-version=2026-10-01", request.Uri.ToUri().Query);
        Assert.EndsWith("/operationStatuses/operation", request.Uri.ToUri().AbsolutePath);
        Assert.True(request.Headers.TryGetValue("x-ms-rhub-routing-location", out string? routing));
        Assert.Equal("eastus2euap", routing);
        Assert.True(request.Headers.TryGetValue("x-recording-id", out string? recording));
        Assert.Equal("recording", recording);
        Assert.Same(content, request.Content);
    }

    [Theory]
    [InlineData("https://management.usgovcloudapi.net/path")]
    [InlineData("https://example.org/path")]
    [InlineData("http://management.azure.com/path")]
    public void RejectsUnexpectedEndpoint(string endpoint)
    {
        using Request request = HttpClientTransport.Shared.CreateRequest();
        request.Uri.Reset(new Uri(endpoint));
        using var message = new HttpMessage(request, new ResponseClassifier());
        Assert.Throws<InvalidOperationException>(() => new GoalTestRoutingPolicy().OnSendingRequest(message));
    }
}
#endif
