// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationTypeListCommandTests : CommandUnitTestsBase<RecommendationTypeListCommand, IAdvisorService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("--resource-type microsoft.compute/virtualmachines", true)]
    [InlineData("--impact High", true)]
    [InlineData("--impact medium", true)]
    [InlineData("--impact LOW", true)]
    [InlineData("--category Cost", true)]
    [InlineData("--resource-type microsoft.sql/servers --impact High --category Security", true)]
    [InlineData("--impact Critical", false)]
    [InlineData("--impact bogus", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("Allowed values", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecommendationTypesList()
    {
        var expected = new List<RecommendationType>
        {
            new("e10b1381-5f0a-47ff-8c7b-37bd13d7c974",
                "Right-size or shutdown underutilized virtual machines",
                Category: "Cost",
                Impact: "High",
                ResourceType: "microsoft.compute/virtualmachines",
                SubCategory: "UsageOptimization"),
            new("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
                "Enable backups on VMs",
                Category: "HighAvailability",
                Impact: "Medium",
                ResourceType: "microsoft.compute/virtualmachines",
                SubCategory: null),
        };
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync();

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationTypeListResult);

        Assert.Equal(expected.Count, result.RecommendationTypes.Count);
        Assert.Equal(expected[0].Id, result.RecommendationTypes[0].Id);
        Assert.Equal(expected[0].DisplayName, result.RecommendationTypes[0].DisplayName);
        Assert.Equal("Cost", result.RecommendationTypes[0].Category);
        Assert.Equal("High", result.RecommendationTypes[0].Impact);
        Assert.Equal("microsoft.compute/virtualmachines", result.RecommendationTypes[0].ResourceType);
        Assert.Equal("UsageOptimization", result.RecommendationTypes[0].SubCategory);
        Assert.Null(result.RecommendationTypes[1].SubCategory);

        await Service.Received(1).ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoRecommendationTypes()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync();

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationTypeListResult);

        Assert.Empty(result.RecommendationTypes);
    }

    [Fact]
    public async Task ExecuteAsync_PassesFiltersToService()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        await ExecuteCommandAsync("--resource-type", "microsoft.compute/virtualmachines",
                                  "--impact", "High",
                                  "--category", "Cost");

        await Service.Received(1).ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            "microsoft.compute/virtualmachines",
            "High",
            "Cost",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesNullsWhenNoFilters()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        await ExecuteCommandAsync();

        // Use Arg.Is for all string params to satisfy NSubstitute's same-type matcher rules.
        await Service.Received(1).ListRecommendationTypesAsync(
            Arg.Is<string?>(v => v == null),
            Arg.Is<string?>(v => v == null),
            Arg.Is<string?>(v => v == null),
            Arg.Is<string?>(v => v == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidImpact_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--impact", "Critical");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Critical", response.Message);
        Assert.Contains("High", response.Message);
        Assert.Contains("Medium", response.Message);
        Assert.Contains("Low", response.Message);

        await Service.DidNotReceive().ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        var forbidden = new HttpRequestException("Forbidden", inner: null, HttpStatusCode.Forbidden);
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbidden);

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        var notFound = new HttpRequestException("Not found", inner: null, HttpStatusCode.NotFound);
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFound);

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("metadata endpoint", response.Message);
    }
}
