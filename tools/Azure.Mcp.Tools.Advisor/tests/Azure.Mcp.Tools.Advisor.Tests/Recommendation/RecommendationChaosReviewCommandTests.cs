// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Advisor.Commands;
using Azure.Mcp.Tools.Advisor.Commands.Recommendation;
using Azure.Mcp.Tools.Advisor.Models.Chaos;
using Azure.Mcp.Tools.Advisor.Options.Recommendation;
using Azure.Mcp.Tools.Advisor.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Advisor.Tests.Recommendation;

public class RecommendationChaosReviewCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationChaosReviewCommand, IAdvisorChaosReviewService>
{
    private const string RecommendationTypeId =
        "11111111-1111-1111-1111-111111111111";
    private const string Resource =
        "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Compute/virtualMachineScaleSets/vmss";
    private const string Workspace =
        "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Chaos/workspaces/workspace";
    private const string Scenario = Workspace + "/scenarios/ComputeZoneDown";
    private const string Configuration = Scenario + "/configurations/zone-down";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();

        Assert.Equal("chaos-review", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData(
        "--subscription sub --recommendation-type-id 11111111-1111-1111-1111-111111111111 --resource /subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Compute/virtualMachineScaleSets/vmss",
        true)]
    [InlineData("--subscription sub", false)]
    [InlineData(
        "--subscription sub --recommendation-type-id invalid --resource /subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Compute/virtualMachineScaleSets/vmss",
        false)]
    [InlineData(
        "--subscription sub --recommendation-type-id 11111111-1111-1111-1111-111111111111 --resource /subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm",
        false)]
    public async Task ExecuteAsync_ValidatesRequiredInput(
        string args,
        bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ReviewChaosRemediationAsync(
                Arg.Any<string>(),
                Arg.Any<Guid>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(CreateStatus());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(
            shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
            response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsInvalidOptionalResourceId()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--recommendation-type-id", RecommendationTypeId,
            "--resource", Resource,
            "--workspace", "https://example.com/workspace");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--workspace", response.Message);
        await Service.DidNotReceiveWithAnyArgs().ReviewChaosRemediationAsync(
            default!,
            default,
            default!,
            default,
            default,
            default,
            default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsEncodedDotSegmentInResourceId()
    {
        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--recommendation-type-id", RecommendationTypeId,
            "--resource",
            "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Compute/virtualMachineScaleSets/%2e%2e");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("--resource", response.Message);
        await Service.DidNotReceiveWithAnyArgs().ReviewChaosRemediationAsync(
            default!,
            default,
            default!,
            default,
            default,
            default,
            default,
            TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_RejectsMismatchedSelectionHierarchy()
    {
        var otherWorkspace =
            "/subscriptions/22222222-2222-2222-2222-222222222222/resourceGroups/rg/providers/Microsoft.Chaos/workspaces/other";

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--recommendation-type-id", RecommendationTypeId,
            "--resource", Resource,
            "--workspace", otherWorkspace,
            "--scenario", Scenario);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("child of the selected --workspace", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ForwardsSelectionsAndTenant()
    {
        Service.ReviewChaosRemediationAsync(
            "sub",
            Guid.Parse(RecommendationTypeId),
            Resource,
            Workspace,
            Scenario,
            Configuration,
            "tenant",
            Arg.Any<CancellationToken>())
            .Returns(CreateStatus());

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--tenant", "tenant",
            "--recommendation-type-id", RecommendationTypeId,
            "--resource", Resource,
            "--workspace", Workspace,
            "--scenario", Scenario,
            "--configuration", Configuration);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ReviewChaosRemediationAsync(
            "sub",
            Guid.Parse(RecommendationTypeId),
            Resource,
            Workspace,
            Scenario,
            Configuration,
            "tenant",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTypedReview()
    {
        var expected = CreateStatus();
        Service.ReviewChaosRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(expected);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--recommendation-type-id", RecommendationTypeId,
            "--resource", Resource);
        var result = ValidateAndDeserializeResponse(
            response,
            AdvisorJsonContext.Default.RecommendationChaosReviewResult);

        Assert.Equal("Ready", result.Review.Status);
        Assert.True(result.Review.Ready);
        Assert.False(result.Review.MutationPerformed);
        Assert.Equal(Resource, result.Review.Target.ResourceId);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceFailure()
    {
        Service.ReviewChaosRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test failure"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub",
            "--recommendation-type-id", RecommendationTypeId,
            "--resource", Resource);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test failure", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Service.ReviewChaosRemediationAsync(
            Arg.Any<string>(),
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new OperationCanceledException(cancellation.Token));

        var options = new RecommendationChaosReviewOptions
        {
            Subscription = "sub",
            RecommendationTypeId = RecommendationTypeId,
            Resource = Resource,
        };

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => Command.ExecuteAsync(Context, options, cancellation.Token));
    }

    private static ChaosRemediationStatus CreateStatus() =>
        new()
        {
            Status = "Ready",
            Ready = true,
            Message = "Ready for review.",
            Target = new(
                "Eligible",
                true,
                null,
                "Eligible.",
                RecommendationTypeId,
                Resource,
                "eastus",
                ["1", "2"],
                2,
                "Succeeded"),
        };
}
