// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationListCommandTests : SubscriptionCommandUnitTestsBase<RecommendationListCommand, IAdvisorService>
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
                Arg.Any<string?>(),
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
            new(new Models.RecommendationProperties(Category: "HighAvailability"), Id: "recId1"),
            new(new Models.RecommendationProperties(Category: "Cost"), Id: "recId2"),
            new(new Models.RecommendationProperties(Category: "Performance"), Id: "recId3")
        };
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>(expectedRecommendations, false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationListResult);

        Assert.Equal(expectedRecommendations.Count, result.Recommendations.Count);
        Assert.Equal(expectedRecommendations[0].Id, result.Recommendations[0].Id);
        Assert.Equal(expectedRecommendations[0].Properties.Category, result.Recommendations[0].Properties.Category);

        // Verify the mock was called
        await Service.Received(1).ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
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
            Arg.Any<string?>(),
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
            Arg.Any<string?>(),
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
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
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
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--category", "Security",
            "--impact", "High",
            "--recommendation-type-id", "1D70919C-1A4A-4F79-8300-BB576C291E9D",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--resource", "mystorage",
            "--search", "encryption");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal("Security", captured!.Category);
        Assert.Equal("High", captured.Impact);
        Assert.Equal("1d70919c-1a4a-4f79-8300-bb576c291e9d", captured.RecommendationTypeId);
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
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Null(captured!.Category);
        Assert.Null(captured.Impact);
        Assert.Null(captured.RecommendationTypeId);
        Assert.Null(captured.ResourceType);
        Assert.Null(captured.Resource);
        Assert.Null(captured.Search);
    }

    [Theory]
    [InlineData("--recommendation-type-id", "not-a-guid")]
    [InlineData("--recommendation-type-id", "{1d70919c-1a4a-4f79-8300-bb576c291e9d}")]
    [InlineData("--category", "Unknown")]
    [InlineData("--impact", "Critical")]
    public async Task ExecuteAsync_InvalidClosedFilter_ReturnsBadRequest(string option, string value)
    {
        var response = await ExecuteCommandAsync("--subscription", "sub123", option, value);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(option, response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_RecommendationTypeId_TrimsAndNormalizesGuid()
    {
        Models.RecommendationFilters? captured = null;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Do<Models.RecommendationFilters?>(f => captured = f),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--recommendation-type-id", "  1D70919C-1A4A-4F79-8300-BB576C291E9D  ");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("1d70919c-1a4a-4f79-8300-bb576c291e9d", captured!.RecommendationTypeId);
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
            Arg.Any<string?>(),
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

    [Fact]
    public async Task ExecuteAsync_ForwardsMetadataFiltersToService()
    {
        // Arrange
        Models.RecommendationFilters? captured = null;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Do<Models.RecommendationFilters?>(f => captured = f),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--sub-category", "ServiceUpgradeAndRetirement",
            "--tracking-ids", "QNY1-HB8",
            "--retirement-date", "ge:2026-03-31");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal("ServiceUpgradeAndRetirement", captured!.SubCategory);
        Assert.Equal(["QNY1-HB8"], captured.TrackingIds);
        Assert.Equal("ge", captured.RetirementDateOperator);
        Assert.Equal(new DateOnly(2026, 3, 31), captured.RetirementDate);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsMultipleTrackingIdsToService()
    {
        // Arrange
        Models.RecommendationFilters? captured = null;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Do<Models.RecommendationFilters?>(f => captured = f),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--tracking-ids", "QNY1-HB8", "9G0V-_G8", "ABC1-D23");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Equal(["QNY1-HB8", "9G0V-_G8", "ABC1-D23"], captured!.TrackingIds);
    }

    [Fact]
    public async Task ExecuteAsync_OmittedMetadataFiltersAreNull()
    {
        // Arrange
        Models.RecommendationFilters? captured = null;
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Do<Models.RecommendationFilters?>(f => captured = f),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(captured);
        Assert.Null(captured!.SubCategory);
        Assert.Null(captured.TrackingIds);
        Assert.Null(captured.RetirementDateOperator);
        Assert.Null(captured.RetirementDate);
    }

    [Theory]
    [InlineData("2026-03-31")]          // Missing operator
    [InlineData("between:2026-03-31")]  // Unsupported operator
    [InlineData("ge:31-03-2026")]       // Wrong date format
    [InlineData("ge:not-a-date")]
    public async Task ExecuteAsync_InvalidRetirementDate_ReturnsBadRequest(string retirementDate)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--retirement-date", retirementDate);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--retirement-date", response.Message);
    }

    [Theory]
    [InlineData("--tracking-ids", "QNY1-HB8")]
    [InlineData("--retirement-date", "ge:2026-03-31")]
    public async Task ExecuteAsync_ConflictingSubCategory_ReturnsBadRequest(string option, string value)
    {
        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--sub-category", "ZoneResiliency",
            option, value);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("ServiceUpgradeAndRetirement", response.Message);
    }

    [Theory]
    [InlineData("--sub-category", "ZoneResiliency")]
    [InlineData("--tracking-ids", "QNY1-HB8")]
    [InlineData("--retirement-date", "ge:2026-03-31")]
    public async Task ExecuteAsync_SecurityMetadataFilters_ReturnBadRequest(
        string option,
        string value)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--category", "Security",
            option,
            value);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("not applicable to Security", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceRetirementFiltersWithoutExplicitSubCategory_AreAccepted()
    {
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--tracking-ids", "QNY1-HB8");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RetirementDateWithoutExplicitSubCategory_IsAccepted()
    {
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--retirement-date", "ge:2026-03-31");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_SecurityCategoryWithValidNonMetadataFilters_IsAccepted()
    {
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--category", "Security",
            "--impact", "High",
            "--resource-type", "Microsoft.Storage/storageAccounts",
            "--search", "encryption");

        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_MatchingSubCategory_IsAccepted()
    {
        // Arrange
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>([], false));

        // Act
        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--sub-category", "serviceupgradeandretirement",
            "--tracking-ids", "QNY1-HB8");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMetadataEnrichedFields()
    {
        // Arrange
        var recommendations = new List<Models.Recommendation>
        {
            new(
                new Models.RecommendationProperties(
                    Category: "HighAvailability",
                    Impact: "High",
                    RecommendationTypeId: "Type-A",
                    RecommendationStatus: "New",
                    CreatedTime: new DateTimeOffset(2026, 5, 13, 3, 19, 48, TimeSpan.Zero),
                    ShortDescription: new Models.RecommendationShortDescription(
                        "Migrate off the retiring feature",
                        "Move to the replacement SKU")),
                Id: "resId1")
        };
        Service.ListRecommendationsAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<Models.RecommendationFilters?>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<Models.Recommendation>(recommendations, false));

        // Act
        var response = await ExecuteCommandAsync("--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationListResult);

        var recommendation = Assert.Single(result.Recommendations);
        Assert.Equal("Type-A", recommendation.Properties.RecommendationTypeId);
        Assert.Equal("HighAvailability", recommendation.Properties.Category);
        Assert.Equal("High", recommendation.Properties.Impact);
        Assert.Equal("New", recommendation.Properties.RecommendationStatus);
        Assert.Equal(new DateTimeOffset(2026, 5, 13, 3, 19, 48, TimeSpan.Zero), recommendation.Properties.CreatedTime);
        Assert.NotNull(recommendation.Properties.ShortDescription);
        Assert.Equal("Migrate off the retiring feature", recommendation.Properties.ShortDescription!.Problem);
        Assert.Equal("Move to the replacement SKU", recommendation.Properties.ShortDescription.Solution);
    }
}
