// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationUpdateCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationUpdateCommand, IAdvisorService>
{
    private static readonly Models.Recommendation UpdatedRecommendation = new(
        Properties: new Models.RecommendationProperties(
            Category: "HighAvailability",
            Impact: "High",
            RecommendationStatus: nameof(RecommendationStatus.Completed),
            ShortDescription: new("Enable availability zones", "Deploy across zones"),
            ResourceMetadata: new(
                "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm")),
        Id: "/subscriptions/sub/providers/Microsoft.Advisor/recommendations/rec-1",
        Name: "rec-1",
        Type: "Microsoft.Advisor/recommendations");

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("--subscription", command.Description);
    }

    [Fact]
    public void Command_HasCorrectMetadata()
    {
        Assert.True(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.Idempotent);
        Assert.False(Command.Metadata.OpenWorld);
        Assert.False(Command.Metadata.ReadOnly);
        Assert.False(Command.Metadata.Secret);
        Assert.False(Command.Metadata.LocalRequired);
    }

    [Theory]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status New", true)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Completed", true)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Dismissed --recommendation-dismiss-reason Other", true)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Dismissed", true)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Completed --recommendation-dismiss-reason RiskIsAcceptable", false)]
    [InlineData("--subscription sub1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --recommendation-id rec-1", false)]
    [InlineData("--recommendation-id rec-1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Invalid", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            ConfigureSuccessfulUpdate();
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PostponedWithFutureDate_ForwardsAllArguments()
    {
        var postponedUntil = DateTimeOffset.UtcNow.AddDays(30);
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", postponedUntil.ToString("O"),
            "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateRecommendationAsync(
            "sub1",
            "rec-1",
            RecommendationStatus.Postponed,
            Arg.Is<DateTimeOffset?>(value => value == postponedUntil),
            null,
            "tenant1",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_PostponedWithoutFutureDate_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O"));

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must be in the future", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PostponedWithoutTimezoneOffset_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", "2099-01-01T12:30:00");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must end in 'Z' or an explicit timezone offset", response.Message);
        await Service.DidNotReceive().UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("New")]
    [InlineData("Completed")]
    [InlineData("Dismissed")]
    public async Task ExecuteAsync_PostponementDateForNonPostponedStatus_ReturnsBadRequest(
        string recommendationStatus)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", recommendationStatus,
            "--postponed-until-date-time", "2099-01-01T12:30:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(
            "--postponed-until-date-time can only be used when --recommendation-status is Postponed",
            response.Message);
        await Service.DidNotReceive().UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_DismissReasonForCompleted_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed",
            "--recommendation-dismiss-reason", "RiskIsAcceptable");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains(
            "--recommendation-dismiss-reason can only be used when --recommendation-status is Dismissed",
            response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Dismissed_ForwardsDismissReason()
    {
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Dismissed",
            "--recommendation-dismiss-reason", "RiskIsAcceptable");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateRecommendationAsync(
            "sub1",
            "rec-1",
            RecommendationStatus.Dismissed,
            null,
            RecommendationDismissReason.RiskIsAcceptable,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsUpdatedRecommendation()
    {
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationUpdateResult);

        Assert.Equal(UpdatedRecommendation, result.Recommendation);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Advisor rejected")]
    [InlineData(HttpStatusCode.Forbidden, "Authorization failed")]
    [InlineData(HttpStatusCode.NotFound, "not found")]
    [InlineData(HttpStatusCode.Conflict, "modified concurrently")]
    public async Task ExecuteAsync_HandlesRequestFailure(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        Service.UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)statusCode, "Backend error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(statusCode, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Backend error", response.Message);
    }

    [Theory]
    [InlineData("SecurityRecommendationStateChangeBlocked", "Security category")]
    [InlineData("UndefinedRecommendationStateChangeBlocked", "Undefined")]
    [InlineData("ResolvedRecommendationStateChangeBlocked", "resolved by the Advisor platform")]
    [InlineData("InvalidRequestPayload", "request payload was invalid")]
    [InlineData("InvalidSubscriptionId", "Verify --subscription")]
    [InlineData("InvalidRecommendationId", "Verify --recommendation-id")]
    [InlineData("RecommendationNotFound", "not found")]
    [InlineData("ConcurrentModification", "modified concurrently")]
    public async Task ExecuteAsync_KnownLifecycleFailure_ReturnsActionableMessage(
        string errorCode,
        string expectedMessage)
    {
        Service.UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                errorCode == "RecommendationNotFound" ? 404 :
                errorCode == "ConcurrentModification" ? 409 : 400,
                "Backend explanation",
                errorCode,
                null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Backend explanation", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DismissedWithoutReason_DefaultsToOther()
    {
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Dismissed");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateRecommendationAsync(
            "sub1",
            "rec-1",
            RecommendationStatus.Dismissed,
            null,
            RecommendationDismissReason.Other,
            null,
            Arg.Any<CancellationToken>());
    }

    private void ConfigureSuccessfulUpdate()
    {
        Service.UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(UpdatedRecommendation);
    }
}
