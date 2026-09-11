// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Metadata;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Metadata;

public class MetadataGetCommandTests : CommandUnitTestsBase<MetadataGetCommand, IAdvisorService>
{
    private const string RecommendationTypeId = "b131ddbe-5439-4c87-95bc-6999b0648252";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252", true)]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252 --language en", true)]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252 --language fr", true)]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252 --language en-US", true)]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252 --language pt-br", true)]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252 --language xx", false)]
    [InlineData("--recommendation-type-id b131ddbe-5439-4c87-95bc-6999b0648252 --language zh", false)]
    [InlineData("--language en", false)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns((RecommendationMetadata?)null);

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMetadata()
    {
        var expected = new RecommendationMetadata(
            RecommendationTypeId: RecommendationTypeId,
            DisplayName: "Service Fabric support for Windows Server 2022 is ending",
            Label: "Service Fabric support for Windows Server 2022 is ending",
            Category: "HighAvailability",
            SubCategory: "ServiceUpgradeAndRetirement",
            Impact: "Medium",
            PriorityScore: 0.631,
            PotentialBenefits: "Avoid potential disruptions",
            DetailedDescription: "Upgrade all Service Fabric clusters to Windows Server 2025.",
            LearnMoreLink: "https://azure.microsoft.com/updates/?id=558247",
            SupportedResourceType: "microsoft.compute/virtualmachinescalesets",
            Scope: "Public",
            DataSourceQuery: "resources | where type == 'microsoft.compute/virtualmachinescalesets'",
            ResourceSingularName: "Virtual machine scale set",
            ResourcePluralName: "Virtual machine scale sets",
            Actions:
            [
                new RecommendationMetadataAction(
                    ActionType: "Document",
                    Caption: "Scale up a Service Fabric primary node type",
                    DocumentLink: "https://learn.microsoft.com/azure/service-fabric/service-fabric-scale-up-primary-node-type",
                    BladeName: null),
            ],
            Language: "en",
            LastRefreshed: "2026-07-27T08:56:16.9945519Z",
            ServiceRetirement: new RecommendationServiceRetirement(
                RetirementDate: "2027-03-31",
                RetirementFeatureName: "Service Fabric support of Windows Server 2022",
                TrackingIds: ["9G0V-_G8"],
                AshUrls: ["https://app.azure.com/h/9G0V-_G8/"]));

        Service.GetRecommendationMetadataAsync(
            RecommendationTypeId,
            "en",
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.MetadataGetResult);

        var metadata = Assert.Single(result.Metadata);
        Assert.Equal(RecommendationTypeId, metadata.RecommendationTypeId);
        Assert.Equal("Service Fabric support for Windows Server 2022 is ending", metadata.DisplayName);
        Assert.Equal("HighAvailability", metadata.Category);
        Assert.Equal("ServiceUpgradeAndRetirement", metadata.SubCategory);
        Assert.Equal("Medium", metadata.Impact);
        Assert.Equal(0.631, metadata.PriorityScore);
        Assert.Equal("microsoft.compute/virtualmachinescalesets", metadata.SupportedResourceType);
        Assert.Equal("en", metadata.Language);

        var action = Assert.Single(metadata.Actions!);
        Assert.Equal("Document", action.ActionType);
        Assert.Equal("Scale up a Service Fabric primary node type", action.Caption);

        Assert.NotNull(metadata.ServiceRetirement);
        Assert.Equal("2027-03-31", metadata.ServiceRetirement!.RetirementDate);
        Assert.Equal("9G0V-_G8", Assert.Single(metadata.ServiceRetirement.TrackingIds!));

        await Service.Received(1).GetRecommendationMetadataAsync(
            RecommendationTypeId,
            "en",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DefaultsLanguageToEn()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns((RecommendationMetadata?)null);

        await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        await Service.Received(1).GetRecommendationMetadataAsync(
            RecommendationTypeId,
            "en",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PassesLanguageToService()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns((RecommendationMetadata?)null);

        await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId, "--language", "fr");

        await Service.Received(1).GetRecommendationMetadataAsync(
            RecommendationTypeId,
            "fr",
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("en-US", "en")]
    [InlineData("EN", "en")]
    [InlineData("fr-FR", "fr")]
    [InlineData("PT-BR", "pt-br")]
    [InlineData("zh-Hant", "zh-hant")]
    [InlineData(" en ", "en")]
    [InlineData("  fr-FR  ", "fr")]
    [InlineData("   ", "en")]
    public async Task ExecuteAsync_NormalizesLanguageToMetadataValue(string input, string expected)
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns((RecommendationMetadata?)null);

        await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId, "--language", input);

        await Service.Received(1).GetRecommendationMetadataAsync(
            RecommendationTypeId,
            expected,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("xx")]
    [InlineData("zh")]
    [InlineData("pt")]
    [InlineData("en_US")]
    public async Task ExecuteAsync_UnsupportedLanguage_ReturnsBadRequest(string language)
    {
        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId, "--language", language);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Unsupported language", response.Message);

        await Service.DidNotReceive().GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmptyWhenNotFound()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .Returns((RecommendationMetadata?)null);

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.MetadataGetResult);

        Assert.Empty(result.Metadata);
    }

    [Fact]
    public async Task ExecuteAsync_MissingRecommendationTypeId_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync("--language", "en");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);

        await Service.DidNotReceive().GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("b131ddbe-5439-4c87-95bc-6999b0648252x")]
    [InlineData("b131ddbe-5439-4c87-95bc-6999b064825z")]
    public async Task ExecuteAsync_InvalidRecommendationTypeId_ReturnsBadRequest(string recommendationTypeId)
    {
        var response = await ExecuteCommandAsync("--recommendation-type-id", recommendationTypeId);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("36-character GUID", response.Message);

        await Service.DidNotReceive().GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles404NotFound()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Not found"));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_Handles403Forbidden()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Forbidden"));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Handles401Unauthorized()
    {
        Service.GetRecommendationMetadataAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Unauthorized, "Authentication failed"));

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        Assert.Equal(HttpStatusCode.Unauthorized, response.Status);
        Assert.Contains("Authentication failed", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMetadata_WithNullOptionalFields()
    {
        // A non-retirement recommendation type: no actions and no service-retirement block.
        var expected = new RecommendationMetadata(
            RecommendationTypeId: RecommendationTypeId,
            DisplayName: "Enable soft delete on storage accounts",
            Label: null,
            Category: "OperationalExcellence",
            SubCategory: null,
            Impact: "Low",
            PriorityScore: null,
            PotentialBenefits: null,
            DetailedDescription: null,
            LearnMoreLink: null,
            SupportedResourceType: "microsoft.storage/storageaccounts",
            Scope: null,
            DataSourceQuery: null,
            ResourceSingularName: null,
            ResourcePluralName: null,
            Actions: null,
            Language: "en",
            LastRefreshed: null,
            ServiceRetirement: null);

        Service.GetRecommendationMetadataAsync(
            RecommendationTypeId,
            "en",
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync("--recommendation-type-id", RecommendationTypeId);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.MetadataGetResult);

        var metadata = Assert.Single(result.Metadata);
        Assert.Equal(RecommendationTypeId, metadata.RecommendationTypeId);
        Assert.Equal("OperationalExcellence", metadata.Category);
        Assert.Null(metadata.SubCategory);
        Assert.Null(metadata.Actions);
        Assert.Null(metadata.ServiceRetirement);
    }
}
