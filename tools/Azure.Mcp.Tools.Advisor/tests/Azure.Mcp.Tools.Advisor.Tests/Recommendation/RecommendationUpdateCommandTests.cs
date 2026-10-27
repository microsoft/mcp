// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationUpdateCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationUpdateCommand, IAdvisorService>
{
    public static TheoryData<string> ValidServiceGroupIds => new()
    {
        "a",
        "service-group-test",
        "AZaz09-_().~",
        "service\u00E9~01",
        "Trey Research Shared Services",
        "Owner's Group",
        "Team #1",
        "/providers/Microsoft.Management/serviceGroups/service-group-test",
        new string('a', 250),
    };

    public static TheoryData<string> InvalidServiceGroupIds => new()
    {
        "bad\u0000name",
        "../group",
        new string('a', 251),
    };

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
    [InlineData("sub1", "")]
    [InlineData("sub1", " ")]
    [InlineData("", "sg1")]
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
    [MemberData(nameof(ValidServiceGroupIds))]
    public async Task ExecuteAsync_ValidServiceGroupId_ForwardsValue(string serviceGroup)
    {
        ConfigureSuccessfulUpdate();

        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).UpdateServiceGroupRecommendationAsync(
            serviceGroup,
            "rec-1",
            RecommendationStatus.Completed,
            null,
            null,
            null,
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [MemberData(nameof(InvalidServiceGroupIds))]
    public async Task ExecuteAsync_InvalidServiceGroupId_ReturnsBadRequest(string serviceGroup)
    {
        var response = await ExecuteCommandAsync(
            "--service-group", serviceGroup,
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Service Group name or ID", response.Message, StringComparison.OrdinalIgnoreCase);
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
        Assert.Null(result.Results);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(result, AdvisorJsonContext.Default.RecommendationUpdateResult));
        Assert.True(json.RootElement.TryGetProperty("recommendation", out _));
        Assert.False(json.RootElement.TryGetProperty("results", out _));
    }

    [Theory]
    [InlineData("--recommendation-id rec-1 --recommendation-ids rec-2")]
    [InlineData("--recommendation-ids rec-1 rec-1")]
    [InlineData("--recommendation-ids rec-1 REC-1")]
    [InlineData("--recommendation-ids")]
    [InlineData("--recommendation-ids rec-1 --postponed-until-date-time 2099-01-01T00:00:00Z")]
    public async Task ExecuteAsync_InvalidBatch_DoesNotUpdateAnyRecommendation(string selection)
    {
        var response = await ExecuteCommandAsync(
            $"--subscription sub1 --recommendation-status Completed {selection}");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_BlankBatchId_DoesNotUpdateAnyRecommendation()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-ids", "rec-1", " ",
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData("rec-1", " rec-1 ")]
    [InlineData("rec-1", " REC-1 ")]
    public async Task ExecuteAsync_DuplicateBatchIdsAfterTrimming_DoesNotUpdateAnyRecommendation(
        string first, string second)
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-ids", first, second,
            "--recommendation-status", "Completed");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_Batch_UpdatesOnlySelectedIdsAndPreservesScopeAndDate()
    {
        ConfigureSuccessfulUpdate();
        var until = new DateTimeOffset(2099, 1, 1, 12, 30, 0, TimeSpan.FromHours(5.5));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--tenant", "tenant1",
            "--recommendation-ids", " rec-1 ", "rec-2",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", until.ToString("O"));

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationUpdateResult);
        Assert.Null(result.Recommendation);
        Assert.NotNull(result.Results);
        Assert.Equal(["rec-1", "rec-2"], result.Results.Select(item => item.RecommendationId));
        Assert.All(result.Results, item =>
        {
            Assert.Equal(RecommendationUpdateOutcome.Succeeded, item.Outcome);
            Assert.Equal(UpdatedRecommendation, item.Recommendation);
            Assert.Null(item.Error);
        });
        foreach (var id in new[] { "rec-1", "rec-2" })
        {
            await Service.Received(1).UpdateRecommendationAsync(
                "sub1", id, RecommendationStatus.Postponed, until, null, "tenant1", Arg.Any<CancellationToken>());
        }
        Assert.Equal(2, Service.ReceivedCalls().Count());
    }

    [Theory]
    [InlineData("New")]
    [InlineData("Completed")]
    [InlineData("Dismissed")]
    public async Task ExecuteAsync_ServiceGroupBatch_ReusesExistingScopedUpdate(string status)
    {
        ConfigureSuccessfulUpdate();
        var response = await ExecuteCommandAsync(
            "--service-group", "sg1",
            "--tenant", "tenant1",
            "--recommendation-ids", "rec-1", "rec-2",
            "--recommendation-status", status);

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationUpdateResult);
        Assert.NotNull(result.Results);
        Assert.Equal(2, result.Results.Count);
        var parsedStatus = Enum.Parse<RecommendationStatus>(status);
        foreach (var id in new[] { "rec-1", "rec-2" })
        {
            await Service.Received(1).UpdateServiceGroupRecommendationAsync(
                "sg1", id, parsedStatus, null,
                parsedStatus == RecommendationStatus.Dismissed ? RecommendationDismissReason.Other : null,
                "tenant1", Arg.Any<CancellationToken>());
        }
        SubscriptionResolver.DidNotReceive().ResolveSubscription(Arg.Any<string?>());
        Assert.Equal(2, Service.ReceivedCalls().Count());
    }

    [Fact]
    public async Task ExecuteAsync_BatchDismissal_ForwardsSharedReasonToEveryUpdate()
    {
        ConfigureSuccessfulUpdate();
        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-ids", "rec-1", "rec-2",
            "--recommendation-status", "Dismissed",
            "--recommendation-dismiss-reason", "RiskIsAcceptable");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        foreach (var id in new[] { "rec-1", "rec-2" })
        {
            await Service.Received(1).UpdateRecommendationAsync(
                "sub1", id, RecommendationStatus.Dismissed, null,
                RecommendationDismissReason.RiskIsAcceptable, null, Arg.Any<CancellationToken>());
        }
    }

    [Fact]
    public async Task ExecuteAsync_BatchFailure_PreservesSuccessesAndContinuesWithoutRetry()
    {
        ConfigureSuccessfulUpdate();
        Service.UpdateRecommendationAsync(
            "sub1", "rec-2", RecommendationStatus.Completed, null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Sensitive backend explanation"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-ids", "rec-1", "rec-2", "rec-3",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.MultiStatus);
        Assert.NotNull(result.Results);
        Assert.Equal(
            [RecommendationUpdateOutcome.Succeeded, RecommendationUpdateOutcome.Failed, RecommendationUpdateOutcome.Succeeded],
            result.Results.Select(item => item.Outcome));
        Assert.Equal(403, result.Results[1].StatusCode);
        Assert.Null(result.Results[1].Recommendation);
        Assert.Contains("Authorization failed", result.Results[1].Error);
        Assert.DoesNotContain("Sensitive backend explanation", result.Results[1].Error);
        Assert.Equal(3, Service.ReceivedCalls().Count());
    }

    [Theory]
    [InlineData("transport")]
    [InlineData("io")]
    [InlineData("json")]
    [InlineData("timeout")]
    public async Task ExecuteAsync_UncertainBatchResponse_PreservesSuccessfulUpdates(string failure)
    {
        ConfigureSuccessfulUpdate();
        Exception exception = failure switch
        {
            "transport" => new HttpRequestException("Sensitive response content"),
            "io" => new IOException("Sensitive response content"),
            "json" => new JsonException("Sensitive response content"),
            _ => new TimeoutException("Sensitive response content"),
        };
        Service.UpdateRecommendationAsync(
            "sub1", "rec-2", RecommendationStatus.Completed, null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(exception);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1", "--recommendation-ids", "rec-1", "rec-2", "rec-3",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.MultiStatus);
        Assert.NotNull(result.Results);
        Assert.Equal(
            [RecommendationUpdateOutcome.Succeeded, RecommendationUpdateOutcome.Unverified, RecommendationUpdateOutcome.Succeeded],
            result.Results.Select(item => item.Outcome));
        Assert.Null(result.Results[1].Recommendation);
        Assert.Contains("Read its current state", result.Results[1].Error);
        Assert.DoesNotContain("Sensitive response content", result.Results[1].Error);
        Assert.Contains("2 succeeded; 0 failed; 1 unverified; 0 not attempted", response.Message);
        Assert.Equal(3, Service.ReceivedCalls().Count());
    }

    [Theory]
    [InlineData(0, HttpStatusCode.BadGateway)]
    [InlineData(408, HttpStatusCode.RequestTimeout)]
    [InlineData(500, HttpStatusCode.InternalServerError)]
    [InlineData(503, HttpStatusCode.ServiceUnavailable)]
    public async Task ExecuteAsync_UncertainBackendResponse_UsesUnverifiedAndValidHttpStatus(
        int statusCode, HttpStatusCode expectedStatus)
    {
        Service.UpdateRecommendationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(statusCode, "Sensitive backend content"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1", "--recommendation-ids", "rec-1",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(response, AdvisorJsonContext.Default.RecommendationUpdateResult, expectedStatus);
        var item = Assert.Single(result.Results!);
        Assert.Equal(RecommendationUpdateOutcome.Unverified, item.Outcome);
        Assert.Equal((int)expectedStatus, item.StatusCode);
        Assert.Contains("Read its current state", item.Error);
        Assert.DoesNotContain("Sensitive backend content", item.Error);
        Assert.Single(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_BatchAllFailures_ReturnsFailureStatusAndPerItemErrors()
    {
        Service.UpdateRecommendationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException(403, "Backend explanation"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-ids", "rec-1", "rec-2",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.Forbidden);
        Assert.NotNull(result.Results);
        Assert.All(result.Results, item => Assert.Equal(RecommendationUpdateOutcome.Failed, item.Outcome));
        Assert.Contains("0 succeeded; 2 failed", response.Message);
        Assert.Equal(2, Service.ReceivedCalls().Count());
    }

    [Fact]
    public async Task ExecuteAsync_BatchCancellation_PreservesSuccessAndMarksRemainingNotAttempted()
    {
        ConfigureSuccessfulUpdate();
        Service.UpdateRecommendationAsync(
            "sub1", "rec-2", RecommendationStatus.Completed, null, null, null, Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-ids", "rec-1", "rec-2", "rec-3",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.MultiStatus);
        Assert.NotNull(result.Results);
        Assert.Equal(
            [RecommendationUpdateOutcome.Succeeded, RecommendationUpdateOutcome.Unverified, RecommendationUpdateOutcome.NotAttempted],
            result.Results.Select(item => item.Outcome));
        Assert.Contains("Read its current state", result.Results[1].Error);
        Assert.Equal("rec-3", result.Results[2].RecommendationId);
        Assert.Equal(2, Service.ReceivedCalls().Count());
    }

    [Fact]
    public async Task ExecuteAsync_CanceledBatch_DoesNotStartAnyUpdates()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        await cancellation.CancelAsync();
        var response = await Command.ExecuteAsync(
            new CommandContext(),
            new RecommendationUpdateOptions
            {
                Subscription = "sub1",
                RecommendationIds = ["rec-1", "rec-2"],
                RecommendationStatus = RecommendationStatus.Completed,
            },
            cancellation.Token);

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.RequestTimeout);
        Assert.NotNull(result.Results);
        Assert.Equal(2, result.Results.Count);
        Assert.All(result.Results, item => Assert.Equal(RecommendationUpdateOutcome.NotAttempted, item.Outcome));
        Assert.Empty(Service.ReceivedCalls());
    }

    [Fact]
    public async Task ExecuteAsync_CancellationBetweenUpdates_StopsAndForwardsTheToken()
    {
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        Service.UpdateRecommendationAsync(
            "sub1", "rec-1", RecommendationStatus.Completed, null, null, null, cancellation.Token)
            .Returns(_ =>
            {
                cancellation.Cancel();
                return UpdatedRecommendation;
            });
        var response = await Command.ExecuteAsync(
            new CommandContext(),
            new RecommendationUpdateOptions
            {
                Subscription = "sub1",
                RecommendationIds = ["rec-1", "rec-2"],
                RecommendationStatus = RecommendationStatus.Completed,
            },
            cancellation.Token);

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.MultiStatus);
        Assert.NotNull(result.Results);
        Assert.Equal(
            [RecommendationUpdateOutcome.Succeeded, RecommendationUpdateOutcome.NotAttempted],
            result.Results.Select(item => item.Outcome));
        Assert.Single(Service.ReceivedCalls());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_PreWriteBatchFailure_RemainsFailedWithoutExposingExceptionDetails(bool serviceGroup)
    {
        Service.UpdateRecommendationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Sensitive exception content"));
        Service.UpdateServiceGroupRecommendationAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(), Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Sensitive exception content"));

        var response = await ExecuteCommandAsync(
            serviceGroup ? "--service-group" : "--subscription", serviceGroup ? "sg1" : "sub1",
            "--recommendation-ids", "rec-1",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response, AdvisorJsonContext.Default.RecommendationUpdateResult, HttpStatusCode.UnprocessableEntity);
        Assert.NotNull(result.Results);
        var item = Assert.Single(result.Results);
        Assert.Equal(RecommendationUpdateOutcome.Failed, item.Outcome);
        Assert.Equal(422, item.StatusCode);
        Assert.Contains("0 succeeded; 1 failed; 0 unverified", response.Message);
        Assert.DoesNotContain("Sensitive exception content", item.Error);
        Assert.DoesNotContain("Sensitive exception content", response.Message);
        Assert.Single(Service.ReceivedCalls());
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
