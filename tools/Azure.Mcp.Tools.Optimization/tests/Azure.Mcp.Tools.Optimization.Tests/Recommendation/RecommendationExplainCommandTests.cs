// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Optimization.Commands;
using Azure.Mcp.Tools.Optimization.Commands.Recommendation;
using Azure.Mcp.Tools.Optimization.Models;
using Azure.Mcp.Tools.Optimization.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Optimization.Tests.Recommendation;

public class RecommendationExplainCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationExplainCommand, IOptimizationService>
{
    private const string ValidResourceId =
        "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("explain", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-id /subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1 --target-sku Standard_E2as_v5", true)]
    [InlineData("--subscription sub123 --target-sku Standard_E2as_v5", false)]  // missing resource-id
    [InlineData("--subscription sub123 --resource-id /subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1", true)]  // target-sku optional; auto-derived
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetRecommendationExplanationAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<UtilizationView>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(EmptyResult());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsExplanation()
    {
        Service.GetRecommendationExplanationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UtilizationView>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new RecommendationExplanationResult(
                OptimizationStrings.ExplanationRenderingInstructions,
                1, ValidResourceId, "eastus", "virtualMachine",
                new SkuConfiguration("Standard_D4s_v5", 1, 4, 16, null),
                new SkuConfiguration("Standard_E2as_v5", 1, 2, 16, null),
                new UtilizationThresholds(80, 80, 80),
                null,
                null));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-id", ValidResourceId,
            "--target-sku", "Standard_E2as_v5");

        var result = ValidateAndDeserializeResponse(response, OptimizationJsonContext.Default.RecommendationExplanationResult);
        Assert.Equal(1, result.RecommendationCount);
        Assert.Equal("eastus", result.Location);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetRecommendationExplanationAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UtilizationView>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-id", ValidResourceId,
            "--target-sku", "Standard_E2as_v5");

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }

    private static RecommendationExplanationResult EmptyResult() => new(
        OptimizationStrings.ExplanationRenderingInstructions,
        0, ValidResourceId, null, null, null, null, null, null, null);
}
