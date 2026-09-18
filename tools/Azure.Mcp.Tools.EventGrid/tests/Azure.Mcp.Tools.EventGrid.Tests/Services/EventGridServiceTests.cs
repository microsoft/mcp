// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
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
}
