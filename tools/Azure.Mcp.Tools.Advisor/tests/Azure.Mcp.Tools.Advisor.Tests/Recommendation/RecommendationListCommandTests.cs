// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationListCommandTests : CommandUnitTestsBase<RecommendationListCommand, IAdvisorService>
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
    [InlineData("--subscription sub1 --resource-group rg1", true)]
    [InlineData("--subscription sub1", true)]  // Missing resource-group
    [InlineData("", false)]                    // Missing all required options
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            Service.ListRecommendationsAsync(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<Models.RecommendationFilters?>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<Models.Recommendation>([], false));
        }

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
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
    public async Task ExecuteAsync_ReturnsRecommendationsList()
    {
        // Arrange
        var expectedRecommendations = new List<Models.Recommendation>
        {
            new(ResourceId: "recId1", RecommendationText: "Recommendation 1", Category: "HighAvailability"),
            new(ResourceId: "recId2", RecommendationText: "Recommendation 2", Category: "Cost"),
            new(ResourceId: "recId3", RecommendationText: "Recommendation 3", Category: "Performance")
        };
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>(expectedRecommendations, false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationListResult);

        Assert.Equal(expectedRecommendations.Count, result.Recommendations.Count);
        Assert.Equal(expectedRecommendations[0].ResourceId, result.Recommendations[0].ResourceId);
        Assert.Equal(expectedRecommendations[0].RecommendationText, result.Recommendations[0].RecommendationText);
        Assert.Equal(expectedRecommendations[0].Category, result.Recommendations[0].Category);

        // Verify the mock was called
        await Service.Received(1).ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNoRecommendations()
    {
        // Arrange
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationListResult);

        Assert.Empty(result.Recommendations);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        // Arrange
        var forbiddenException = new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed");
        Service.ListRecommendationsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<Models.RecommendationFilters?>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(forbiddenException);

        // Act
        var response = await ExecuteCommandAsync("--subscription", "test-subscription", "--resource-group", "test-rg");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsFiltersToService()
    {
        // Arrange
        Models.RecommendationFilters? captured = null;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Do<Models.RecommendationFilters?>(f => captured = f),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--category", "Security",
            "--impact", "High",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--resource", "mystorage",
            "--search", "encryption");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal("Security", captured!.Category);
        Assert.Equal("High", captured.Impact);
        Assert.Equal("Microsoft.Storage/storageAccounts", captured.ResourceType);
        Assert.Equal("mystorage", captured.Resource);
        Assert.Equal("encryption", captured.Search);
    }

    [Fact]
    public async Task ExecuteAsync_OmittedFiltersAreNull()
    {
        // Arrange
        Models.RecommendationFilters? captured = null;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Do<Models.RecommendationFilters?>(f => captured = f),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Null(captured!.Category);
        Assert.Null(captured.Impact);
        Assert.Null(captured.ResourceType);
        Assert.Null(captured.Resource);
        Assert.Null(captured.Search);
    }

    [Theory]
    [InlineData(null, 50)]
    [InlineData(10, 10)]
    [InlineData(0, 1)]
    [InlineData(500, 100)]
    public async Task ExecuteAsync_ForwardsTopWithClamping(int? top, int expectedTop)
    {
        // Arrange
        int capturedTop = -1;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Do<int>(t => capturedTop = t),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        var args = top is null
            ? new[] { "--subscription", "sub123" }
            : new[] { "--subscription", "sub123", "--top", top.Value.ToString() };

        // Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal(expectedTop, capturedTop);
    }
}
