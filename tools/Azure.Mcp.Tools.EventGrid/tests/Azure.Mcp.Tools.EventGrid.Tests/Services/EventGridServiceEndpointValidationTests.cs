// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Mcp.Tools.EventGrid.Services;
using Azure.ResourceManager;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.Tests.Services;

public class EventGridServiceEndpointValidationTests
{
    [Theory]
    [InlineData("https://topic.westus2-1.eventgrid.azure.net/api/events", "public")]
    [InlineData("https://topic.chinanorth3-1.eventgrid.azure.cn/api/events", "china")]
    [InlineData("https://topic.usgovvirginia-1.eventgrid.azure.us/api/events", "government")]
    public void ValidateTopicEndpoint_ValidCloudEndpoint_ReturnsEndpoint(string endpoint, string cloud)
    {
        var uri = new Uri(endpoint);
        var armEnvironment = cloud switch
        {
            "china" => ArmEnvironment.AzureChina,
            "government" => ArmEnvironment.AzureGovernment,
            _ => ArmEnvironment.AzurePublicCloud
        };

        Assert.Same(uri, EventGridService.ValidateTopicEndpoint(uri, armEnvironment));
    }

    [Theory]
    [InlineData("https://evil.example/api/events")]
    [InlineData("https://topic.eventgrid.azure.net.evil.example/api/events")]
    [InlineData("http://topic.westus2-1.eventgrid.azure.net/api/events")]
    [InlineData("https://topic.chinanorth3-1.eventgrid.azure.cn/api/events")]
    public void ValidateTopicEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            EventGridService.ValidateTopicEndpoint(new Uri(endpoint), ArmEnvironment.AzurePublicCloud));
    }
}
