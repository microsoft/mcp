// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models;
using Azure.Mcp.Tools.Advisor.Services;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationPatchCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationPatchCommand, IAdvisorService>
{
    private static readonly Models.Recommendation PatchedRecommendation = new(
        ResourceId: "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm",
        RecommendationText: "Enable availability zones",
        Category: "HighAvailability",
        Impact: "High",
        RecommendationId: "/subscriptions/sub/providers/Microsoft.Advisor/recommendations/rec-1",
        StableId: "rec-1",
        RecommendationStatus: nameof(RecommendationStatus.Completed));

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("patch", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
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
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Dismissed", false)]
    [InlineData("--subscription sub1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --recommendation-id rec-1", false)]
    [InlineData("--recommendation-id rec-1 --recommendation-status Completed", false)]
    [InlineData("--subscription sub1 --recommendation-id rec-1 --recommendation-status Invalid", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            ConfigureSuccessfulPatch();
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_PostponedWithFutureDate_ForwardsAllArguments()
    {
        var postponedUntil = DateTimeOffset.UtcNow.AddDays(30);
        ConfigureSuccessfulPatch();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Postponed",
            "--postponed-until-date-time", postponedUntil.ToString("O"),
            "--tenant", "tenant1");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).PatchRecommendationAsync(
            "sub1",
            "rec-1",
            RecommendationStatus.Postponed,
            Arg.Is<DateTimeOffset?>(value => value == postponedUntil),
            null,
            "tenant1",
            Arg.Any<RetryPolicyOptions?>(),
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
    public async Task ExecuteAsync_Dismissed_ForwardsDismissReason()
    {
        ConfigureSuccessfulPatch();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Dismissed",
            "--recommendation-dismiss-reason", "RiskIsAcceptable");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).PatchRecommendationAsync(
            "sub1",
            "rec-1",
            RecommendationStatus.Dismissed,
            null,
            RecommendationDismissReason.RiskIsAcceptable,
            null,
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPatchedRecommendation()
    {
        ConfigureSuccessfulPatch();

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationPatchResult);

        Assert.Equal(PatchedRecommendation, result.Recommendation);
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
        Service.PatchRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)statusCode, "Backend error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub1",
            "--recommendation-id", "rec-1",
            "--recommendation-status", "Completed");

        Assert.Equal(statusCode, response.Status);
        Assert.Contains(expectedMessage, response.Message, StringComparison.OrdinalIgnoreCase);
    }

    private void ConfigureSuccessfulPatch()
    {
        Service.PatchRecommendationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<RecommendationStatus>(),
            Arg.Any<DateTimeOffset?>(),
            Arg.Any<RecommendationDismissReason?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(PatchedRecommendation);
    }
}
