// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Tests.Attributes;
using Microsoft.Mcp.Tests.Client;
using Microsoft.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.Tests;

public class ClusterLiveCommandTests(ITestOutputHelper output, TestProxyFixture fixture, LiveServerFixture liveServerFixture)
    : RecordedCommandTestsBase(output, fixture, liveServerFixture)
{
    [Fact]
    [LiveTestOnly]
    public async Task Should_validate_subscription_id()
    {
        var result = await CallToolAsync(
            "kusto_cluster_get",
            new()
            {
                { "cluster", Settings.ResourceBaseName }
            },
            resultProcessor: elem => elem.TryGetProperty("status", out var property) ? property : null);

        // Assert that we have a result
        Assert.NotNull(result);

        var statusCode = result.Value.GetInt32();
        Assert.Equal(400, statusCode);
    }
}
