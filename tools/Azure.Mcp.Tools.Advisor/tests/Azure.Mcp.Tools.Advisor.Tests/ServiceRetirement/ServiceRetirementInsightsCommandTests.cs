// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.ServiceRetirement;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.ServiceRetirement;

public class ServiceRetirementInsightsCommandTests
    : CommandUnitTestsBase<ServiceRetirementInsightsCommand, IAdvisorService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("insights", command.Name);
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("--subscription sub1", true)]
    [InlineData("--service-group sg1 --top 25", true)]
    [InlineData("--subscription sub1 --service-group sg1", false)]
    [InlineData("--top 0", false)]
    [InlineData("--top 101", false)]
    [InlineData("--subscription \" \"", false)]
    [InlineData("--service-group \" \"", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.ListServiceRetirementInsightsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTypedInsights()
    {
        Service.ListServiceRetirementInsightsAsync(
            "sub1",
            10,
            Arg.Any<CancellationToken>())
            .Returns(
            [
                new(
                    "Subscription One",
                    "sub1",
                    DateTimeOffset.Parse("2026-10-06T12:00:00Z"),
                    10,
                    4,
                    "ServiceRetirement",
                    false,
                    "Recommenddata"),
            ]);

        var response = await ExecuteCommandAsync(
            "--subscription", " sub1 ",
            "--top", "10");
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.ServiceRetirementInsightsResult);

        var insight = Assert.Single(result.Insights);
        Assert.Equal("ServiceRetirement", insight.InsightName);
        Assert.Equal("Subscription One", insight.InsightResourceName);
        Assert.Equal("sub1", insight.InsightResourceId);
        Assert.Equal(DateTimeOffset.Parse("2026-10-06T12:00:00Z"), insight.LastUpdatedTime);
        Assert.Equal(10, insight.Day1ImpactedResources);
        Assert.Equal(4, insight.CurrentImpactedResources);
        Assert.False(insight.IsDeleted);
        Assert.Equal("Recommenddata", insight.Domain);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceGroup_ForwardsNormalizedScope()
    {
        Service.ListServiceRetirementInsightsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(
            "--service-group", " commerce ",
            "--top", "25");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListServiceRetirementInsightsAsync(
            "commerce",
            25,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_FleetWide_UsesDefaultTop()
    {
        Service.ListServiceRetirementInsightsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListServiceRetirementInsightsAsync(
            null,
            100,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListServiceRetirementInsightsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var response = await ExecuteCommandAsync("");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Theory]
    [InlineData(404, HttpStatusCode.NotFound, "data not found")]
    [InlineData(403, HttpStatusCode.Forbidden, "Authorization failed")]
    public async Task ExecuteAsync_RequestFailure_ReturnsSpecificError(
        int status,
        HttpStatusCode expectedStatus,
        string expectedMessage)
    {
        Service.ListServiceRetirementInsightsAsync(
            Arg.Any<string?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(status, "Test request failure"));

        var response = await ExecuteCommandAsync("--subscription sub1");

        Assert.Equal(expectedStatus, response.Status);
        Assert.Contains(expectedMessage, response.Message);
    }
}
