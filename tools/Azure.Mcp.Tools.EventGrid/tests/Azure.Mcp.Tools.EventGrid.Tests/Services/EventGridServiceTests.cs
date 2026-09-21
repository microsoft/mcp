// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using System.Security;
using Azure.Core;
using Azure.Mcp.Tools.EventGrid.Services;
using Azure.ResourceManager;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.Tests.Services;

public class EventGridServiceTests()
{
    [Fact]
    public async Task FindUniqueTopic_NoMatchingTopic_ReturnsNull()
    {
        var result = await FindUniqueTopic(CreateTopic("other-rg", "other-topic"));

        Assert.Null(result);
    }

    [Fact]
    public async Task FindUniqueTopic_OneMatchingTopic_ReturnsTopic()
    {
        var expected = CreateTopic("target-rg", "my-topic");

        var result = await FindUniqueTopic(CreateTopic("other-rg", "other-topic"), expected);

        Assert.Same(expected, result);
    }

    [Fact]
    public async Task FindUniqueTopic_MultipleMatchingTopics_ThrowsWithResourceGroups()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => FindUniqueTopic(
                CreateTopic("rg-prod", "my-topic"),
                CreateTopic("other-rg", "other-topic"),
                CreateTopic("rg-dev", "MY-TOPIC")));

        Assert.Equal(
            "Multiple Event Grid topics named 'my-topic' found in resource groups: rg-prod, rg-dev. Specify a specific --resource-group to disambiguate.",
            exception.Message);
    }

    private static Task<ArmResource?> FindUniqueTopic(params ArmResource[] topics) => EventGridService.FindUniqueTopic(
        GetTopics(topics),
        "my-topic",
        "Event Grid topics",
        CancellationToken.None);

    private static ArmResource CreateTopic(string resourceGroup, string topicName)
    {
        var topic = Substitute.For<ArmResource>();
        topic.Id.Returns(new ResourceIdentifier(
            $"/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/{resourceGroup}/providers/Microsoft.EventGrid/topics/{topicName}"));
        return topic;
    }

    private static async IAsyncEnumerable<ArmResource> GetTopics(
        IEnumerable<ArmResource> topics,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var topic in topics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return topic;
        }

        await Task.CompletedTask;
    }

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

        Assert.Same(uri, EventGridService.ValidateEventGridEndpoint(uri, armEnvironment));
    }

    [Theory]
    [InlineData("https://evil.example/api/events")]
    [InlineData("https://topic.eventgrid.azure.net.evil.example/api/events")]
    [InlineData("http://topic.westus2-1.eventgrid.azure.net/api/events")]
    [InlineData("https://topic.chinanorth3-1.eventgrid.azure.cn/api/events")]
    [InlineData("https://topic.usgovvirginia-1.eventgrid.azure.us/api/events")]
    public void ValidateTopicEndpoint_InvalidPublicCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            EventGridService.ValidateEventGridEndpoint(new Uri(endpoint), ArmEnvironment.AzurePublicCloud));
    }

    [Theory]
    [InlineData("https://evil.example/api/events")]
    [InlineData("https://topic.eventgrid.azure.us.evil.example/api/events")]
    [InlineData("http://topic.usgovvirginia-1.eventgrid.azure.us/api/events")]
    [InlineData("https://topic.westus2-1.eventgrid.azure.net/api/events")]
    [InlineData("https://topic.chinanorth3-1.eventgrid.azure.cn/api/events")]
    public void ValidateTopicEndpoint_InvalidUsGovCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            EventGridService.ValidateEventGridEndpoint(new Uri(endpoint), ArmEnvironment.AzureGovernment));
    }

    [Theory]
    [InlineData("https://evil.example/api/events")]
    [InlineData("https://topic.eventgrid.azure.cn.evil.example/api/events")]
    [InlineData("http://topic.chinanorth3-1.eventgrid.azure.cn/api/events")]
    [InlineData("https://topic.westus2-1.eventgrid.azure.net/api/events")]
    [InlineData("https://topic.usgovvirginia-1.eventgrid.azure.us/api/events")]
    public void ValidateTopicEndpoint_InvalidChinaCloudEndpoint_ThrowsSecurityException(string endpoint)
    {
        Assert.Throws<SecurityException>(() =>
            EventGridService.ValidateEventGridEndpoint(new Uri(endpoint), ArmEnvironment.AzureChina));
    }

    [Fact]
    public void ValidateTopicEndpoint_NullUri_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            EventGridService.ValidateEventGridEndpoint(new Uri("https://evil.example/api/events"), ArmEnvironment.AzureChina));
    }
}
