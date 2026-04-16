// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Nodes;
using Azure.Mcp.Tools.ApplicationInsights.Commands;
using Azure.Mcp.Tools.ApplicationInsights.Commands.Recommendation;
using Azure.Mcp.Tools.ApplicationInsights.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ApplicationInsights.UnitTests;

public class RecommendationListCommandTests : CommandUnitTestsBase<RecommendationListCommand, IApplicationInsightsService>
{
    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsInsights_SetsResults()
    {
        var insights = new List<JsonNode>
        {
            new JsonObject([new("id", "rec1"), new("type", "cpu")]),
            new JsonObject([new("id", "rec2"), new("type", "memory")])
        };
        Service.GetProfilerInsightsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(insights);

        var response = await ExecuteCommandAsync("--subscription", "sub1");

        Assert.NotNull(response.Results);

        var node = ConvertResponse(response, ApplicationInsightsJsonContext.Default.RecommendationListCommandResult);
        var recs = node?.Recommendations;
        Assert.NotNull(recs);
        Assert.Equal(2, recs.Count());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceReturnsNoInsights_NoResults()
    {
        Service.GetProfilerInsightsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", "sub1");
        Assert.Null(response.Results);
    }
}
