// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Azure.Mcp.Tools.EventGrid.Services;
using Xunit;

namespace Azure.Mcp.Tools.EventGrid.Tests.Services;

public class EventGridServiceTests()
{
    [Fact]
    public async Task FindUniqueTopic_NoMatchingTopic_ReturnsNull()
    {
        var result = await FindUniqueTopic("other-rg/other-topic");

        Assert.Null(result);
    }

    [Fact]
    public async Task FindUniqueTopic_OneMatchingTopic_ReturnsTopic()
    {
        var result = await FindUniqueTopic("other-rg/other-topic", "target-rg/my-topic");

        Assert.Equal("target-rg/my-topic", result);
    }

    [Fact]
    public async Task FindUniqueTopic_MultipleMatchingTopics_ThrowsWithResourceGroups()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => FindUniqueTopic("rg-prod/my-topic", "other-rg/other-topic", "rg-dev/MY-TOPIC"));

        Assert.Equal(
            "Multiple Event Grid topics named 'my-topic' found in resource groups: rg-prod, rg-dev. Specify --resource-group to disambiguate.",
            exception.Message);
    }

    private static Task<string?> FindUniqueTopic(params string[] topics) => EventGridService.FindUniqueTopic(
        GetTopics(topics),
        "my-topic",
        "Event Grid topics",
        static topic => topic[(topic.IndexOf('/') + 1)..],
        static topic => topic[..topic.IndexOf('/')],
        CancellationToken.None);

    private static async IAsyncEnumerable<string> GetTopics(
        IEnumerable<string> topics,
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
