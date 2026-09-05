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
        Assert.Contains("--service-group", command.Description);
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
    [InlineData("--service-group sg1 --recommendation-id rec-1 --recommendation-status New", true)]
    [InlineData("--service-group sg1 --recommendation-id rec-1 --recommendation-status Completed", true)]
    [InlineData("--service-group sg1 --recommendation-id rec-1 --recommendation-status Dismissed", true)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Completed --recommendation-dismiss-reason RiskIsAcceptable", false)]
    [InlineData("--subscription sub1 --service-group sg1 --recommendation-id rec-1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --service-group sg1 --tenant tenant1 --recommendation-id rec-1 --recommendation-status Completed", false)]
    [InlineData("--service-group bad/name --recommendation-id rec-1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --recommendation-id rec-1", false)]
    [InlineData("--recommendation-id rec-1 --recommendation-status Completed", false)]
    [InlineData("--tenant tenant1 --recommendation-id rec-1 --recommendation-status Completed", false)]
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
    public async Task ExecuteAsync_WithoutScope_UsesDefaultSubscription()
    {
        SubscriptionResolver.ResolveSubscription(null).Returns("default-subscription");
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateRecommendationAsync(
            "default-subscription",
            "rec-1",
            RecommendationStatus.Completed,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutScopeAndWithTenant_UsesDefaultSubscription()
    {
        SubscriptionResolver.ResolveSubscription(null).Returns("default-subscription");
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--tenant", "tenant1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateRecommendationAsync(
            "default-subscription",
            "rec-1",
            RecommendationStatus.Completed,
            null,
            null,
            "tenant1",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceGroup_ForwardsArgumentsWithoutResolvingSubscription()
    {
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", "2099-01-01T12:30:00+05:30",
            "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateServiceGroupRecommendationAsync(
            "sg1",
            "rec-1",
            RecommendationStatus.Postponed,
            new DateTimeOffset(2099, 1, 1, 12, 30, 0, TimeSpan.FromHours(5.5)),
            null,
            "tenant1",
            Arg.Any<CancellationToken>());
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
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
    public async Task ExecuteAsync_BlankServiceGroup_DoesNotFallBackToDefaultSubscription()
    {
        SubscriptionResolver.ResolveSubscription(null).Returns("default-subscription");

        var response = await ExecuteCommandAsync(
            "--service-group", " ",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        await Service.DidNotReceive().UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await Service.DidNotReceive().UpdateServiceGroupRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("sub1", " ")]
    [InlineData(" ", "sg1")]
    public async Task ExecuteAsync_BothScopeOptionsExplicitlyProvided_RejectsBlankValues(
        string subscription,
        string serviceGroup)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", subscription,
            "--service-group", serviceGroup,
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Specify either --subscription or --service-group, not both", response.Message);
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        await Service.DidNotReceive().UpdateRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
        await Service.DidNotReceive().UpdateServiceGroupRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(RecommendationDismissReason.ExcessiveCostInvestmentRequired)]
    [InlineData(RecommendationDismissReason.ImplementationStepsAreUnclear)]
    [InlineData(RecommendationDismissReason.IncompatibleWithTheCurrentConfiguration)]
    [InlineData(RecommendationDismissReason.RiskIsAcceptable)]
    [InlineData(RecommendationDismissReason.TooComplexOrImpracticalToImplement)]
    [InlineData(RecommendationDismissReason.AnAlternativeSolutionIsAlreadyInPlace)]
    [InlineData(RecommendationDismissReason.Other)]
    public async Task ExecuteAsync_ServiceGroupDismissed_ForwardsEveryDismissReason(
        RecommendationDismissReason dismissReason)
    {
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Dismissed",
            "--recommendation-dismiss-reason", dismissReason.ToString());

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateServiceGroupRecommendationAsync(
            "sg1",
            "rec-1",
            RecommendationStatus.Dismissed,
            null,
            dismissReason,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("bad#name")]
    [InlineData("../group")]
    [InlineData("1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901")]
    public async Task ExecuteAsync_InvalidServiceGroup_ReturnsBadRequest(string serviceGroup)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("service group name", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceive().UpdateServiceGroupRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());
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
    public async Task ExecuteAsync_PostponedWithoutDate_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--postponed-until-date-time is required", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PostponedWithInvalidDate_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", "not-a-dateZ");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("must be a valid ISO 8601 date and time", response.Message);
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
    public async Task ExecuteAsync_BlankRecommendationId_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recommendation-id", " ",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--recommendation-id", response.Message);
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
    public async Task ExecuteAsync_InvalidServiceGroupFailure_ReturnsActionableMessage()
    {
        Service.UpdateServiceGroupRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(
                (int)HttpStatusCode.BadRequest,
                "Backend explanation",
                "InvalidServiceGroupId",
                null));

        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Verify --service-group", response.Message);
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
        Service.UpdateServiceGroupRecommendationAsync(
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
