// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Metadata;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Metadata;

public class RecommendationMetadataListCommandTests
    : CommandUnitTestsBase<RecommendationMetadataListCommand, IAdvisorService>
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
    [InlineData("--language de", true)]
    [InlineData("--language en-US", true)]
    [InlineData("--language fr-CA", true)]
    [InlineData("--language pt-BR", true)]
    [InlineData("--language pt-XX", false)]
    [InlineData("--resource-type microsoft.compute/virtualmachines", true)]
    [InlineData("--impact High", true)]
    [InlineData("--impact medium", true)]
    [InlineData("--category Cost", true)]
    [InlineData("--category Sustainability", true)]
    [InlineData("--impact Critical", false)]
    [InlineData("--language Klingon", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<RecommendationMetadata>([], false));

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMetadataAndTruncationState()
    {
        var expected = CreateMetadata();
        Service.ListRecommendationMetadataAsync(
            "en",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<RecommendationMetadata>([expected], true));

        var response = await ExecuteCommandAsync();
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationMetadataListResult);

        var metadata = Assert.Single(result.Metadata);
        Assert.Equal(expected.RecommendationTypeId, metadata.RecommendationTypeId);
        Assert.Equal(expected.PriorityScore, metadata.PriorityScore);
        Assert.Equal(expected.Actions, metadata.Actions);
        Assert.Equal(expected.ServiceRetirement!.RetirementDate, metadata.ServiceRetirement!.RetirementDate);
        Assert.Equal(expected.ServiceRetirement.TrackingIds, metadata.ServiceRetirement.TrackingIds);
        Assert.True(result.AreResultsTruncated);
    }

    [Fact]
    public async Task ExecuteAsync_NormalizesLanguageAndPassesFilters()
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<RecommendationMetadata>([], false));

        await ExecuteCommandAsync(
            "--language", "en-US",
            "--resource-type", "microsoft.compute/virtualmachines",
            "--impact", "High",
            "--category", "Cost",
            "--tenant", "tenant-id");

        await Service.Received(1).ListRecommendationMetadataAsync(
            "en",
            "microsoft.compute/virtualmachines",
            "High",
            "Cost",
            "tenant-id",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("en-US", "en")]
    [InlineData("fr-CA", "fr")]
    [InlineData("pt-BR", "pt-br")]
    [InlineData("zh-Hans", "zh-hans")]
    public async Task ExecuteAsync_NormalizesSupportedLanguageTags(string input, string expected)
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<RecommendationMetadata>([], false));

        await ExecuteCommandAsync("--language", input);

        await Service.Received(1).ListRecommendationMetadataAsync(
            expected,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CanonicalizesImpact()
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<RecommendationMetadata>([], false));

        await ExecuteCommandAsync("--impact", "medium");

        await Service.Received(1).ListRecommendationMetadataAsync(
            "en",
            null,
            "Medium",
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyMetadata()
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<RecommendationMetadata>([], false));

        var response = await ExecuteCommandAsync();
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationMetadataListResult);

        Assert.Empty(result.Metadata);
        Assert.False(result.AreResultsTruncated);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidImpact_DoesNotCallService()
    {
        var response = await ExecuteCommandAsync("--impact", "Critical");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Allowed values", response.Message);
        await Service.DidNotReceive().ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
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
    public async Task ExecuteAsync_HandlesForbiddenWithoutLeakingBackendMessage()
    {
        Service.ListRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.Forbidden,
                "sensitive backend details"));

        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.DoesNotContain("sensitive backend details", response.Message);
    }

    private static RecommendationMetadata CreateMetadata() => new(
        RecommendationTypeId: "e10b1381-5f0a-47ff-8c7b-37bd13d7c974",
        DisplayName: "Right-size or shutdown underutilized virtual machines",
        Label: "Right-size virtual machines",
        Category: "Cost",
        SubCategory: "UsageOptimization",
        Impact: "High",
        PriorityScore: 0.9,
        PotentialBenefits: "Reduce cost",
        DetailedDescription: "Review underutilized virtual machines.",
        LearnMoreLink: "https://learn.microsoft.com/azure/advisor/",
        SupportedResourceType: "microsoft.compute/virtualmachines",
        Scope: "Public",
        DataSourceQuery: "resources | where type =~ 'microsoft.compute/virtualmachines'",
        ResourceSingularName: "Virtual machine",
        ResourcePluralName: "Virtual machines",
        Actions: [new("Document", "Review guidance", "https://learn.microsoft.com/azure/advisor/", null)],
        Language: "en",
        LastRefreshed: "2026-07-27T08:56:16Z",
        ServiceRetirement: new("2027-03-31", "Legacy feature", ["tracking-id"], ["https://app.azure.com/h/tracking-id/"]));
}
