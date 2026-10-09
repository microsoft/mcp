// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.StatusInsights;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.StatusInsights;

public class StatusInsightsGetCommandTests : CommandUnitTestsBase<StatusInsightsGetCommand, IServiceGroupIntelligenceService>
{
    [Theory]
    [InlineData("", true)]
    [InlineData("--service-group sg1 --include status", true)]
    [InlineData("--criticality-tier 0 --status Critical --insight-name ZonalResiliency", true)]
    [InlineData("--include other", false)]
    [InlineData("--criticality-tier 7", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.GetStatusInsightsAsync(
            Arg.Any<string[]?>(), Arg.Any<string[]?>(), Arg.Any<string[]?>(), Arg.Any<string[]?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ServiceGroupStatusInsightsPage([], false, null));

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceGroups()
    {
        var group = new ServiceGroupStatusInsight("sg1", "/providers/microsoft.management/servicegroups/sg1", "0", "Mission-critical", "At Risk", "desc", "impact", null);
        Service.GetStatusInsightsAsync(
            Arg.Any<string[]?>(), Arg.Any<string[]?>(), Arg.Any<string[]?>(), Arg.Any<string[]?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ServiceGroupStatusInsightsPage([group], true, "tok"));

        var response = await ExecuteCommandAsync("--service-group sg1");
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.StatusInsightsGetResult);

        Assert.Equal("sg1", Assert.Single(result.ServiceGroups).Name);
        Assert.True(result.MoreAvailable);
        Assert.Equal("tok", result.ContinuationToken);
        Assert.NotNull(result.StatusCalculation);
    }
}
