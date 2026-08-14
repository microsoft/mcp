// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Insights.Services;
using ModelContextProtocol.Server;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Insights.UnitTests.Services;

public class SamplingServiceTests
{
    [Fact]
    public async Task SampleTextAsync_NullServer_Throws()
    {
        var service = new SamplingService();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SampleTextAsync(null!, "system", "user", 100, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task SampleTextAsync_EmptyUserPrompt_Throws(string? userPrompt)
    {
        var service = new SamplingService();
        var server = Substitute.For<McpServer>();

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            service.SampleTextAsync(server, "system", userPrompt!, 100, TestContext.Current.CancellationToken));
    }
}
