// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Options;
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
    [InlineData("--subscription sub1", true)]
    [InlineData("--subscription sub1 --filter recommendationType", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListRecommendationTypesAsync(
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsRecommendationTypesList()
    {
        var expected = new List<RecommendationType>
        {
            new("HighAvailability", "High Availability"),
            new("Cost", "Cost"),
            new("Performance", "Performance"),
        };
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationTypeListResult);

        Assert.Equal(expected.Count, result.RecommendationTypes.Count);
        Assert.Equal(expected[0].Id, result.RecommendationTypes[0].Id);
        Assert.Equal(expected[0].DisplayName, result.RecommendationTypes[0].DisplayName);

        await Service.Received(1).ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoRecommendationTypes()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationTypeListResult);

        Assert.Empty(result.RecommendationTypes);
    }

    [Fact]
    public async Task ExecuteAsync_PreservesRichRecommendationTypeFields()
    {
        var expected = new List<RecommendationType>
        {
            new(
                Id: "e10b1381-5f0a-47ff-8c7b-37bd13d7c974",
                DisplayName: "Right-size or shutdown underutilized virtual machines",
                Category: "Cost",
                Impact: "High",
                SubCategory: "UsageOptimization",
                ResourceType: "microsoft.compute/virtualmachines"),
        };
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--filter", "recommendationType");

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationTypeListResult);

        var item = Assert.Single(result.RecommendationTypes);
        Assert.Equal("e10b1381-5f0a-47ff-8c7b-37bd13d7c974", item.Id);
        Assert.Equal("Right-size or shutdown underutilized virtual machines", item.DisplayName);
        Assert.Equal("Cost", item.Category);
        Assert.Equal("High", item.Impact);
        Assert.Equal("UsageOptimization", item.SubCategory);
        Assert.Equal("microsoft.compute/virtualmachines", item.ResourceType);
    }

    [Fact]
    public async Task ExecuteAsync_PassesFilterToService()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        await ExecuteCommandAsync("--subscription", "sub123", "--filter", "category");

        await Service.Received(1).ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            "category",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListRecommendationTypesAsync(
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123");

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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(forbidden);

        var response = await ExecuteCommandAsync("--subscription", "sub123");

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
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFound);

        var response = await ExecuteCommandAsync("--subscription", "sub123");

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("metadata endpoint", response.Message);
    }
}
