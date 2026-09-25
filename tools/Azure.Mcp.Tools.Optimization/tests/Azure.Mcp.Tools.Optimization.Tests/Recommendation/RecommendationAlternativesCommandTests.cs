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

public class RecommendationAlternativesCommandTests
    : SubscriptionCommandUnitTestsBase<RecommendationAlternativesCommand, IOptimizationService>
{
    private const string ValidResourceId =
        "/subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1";

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = Command.GetCommand();
        Assert.Equal("alternatives", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-id /subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1", true)]
    [InlineData("--subscription sub123 --resource-id not-a-valid-id", false)]
    [InlineData("--subscription sub123", false)]  // missing resource-id
    [InlineData("--resource-id /subscriptions/12345678-1234-1234-1234-123456789012/resourceGroups/rg/providers/Microsoft.Compute/virtualMachines/vm1", false)]  // missing subscription
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.GetAlternativesAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
                .Returns(new List<AlternativeRecommendation>());
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMarkdownAndAlternatives()
    {
        var alternatives = new List<AlternativeRecommendation>
        {
            new()
            {
                ResourceId = ValidResourceId,
                Option = 1,
                ObservationWindowDays = 7,
                RecommendationMessage = "Resize",
                ProposedSku = "Standard_D2s_v5",
                ProposedSeries = "Dsv5",
                ProposedProcessor = "Intel",
                EstimatedMonthlySavings = 42,
                SavingsCurrency = "USD",
                EstimatedCoresSavings = 2,
            },
        };
        Service.GetAlternativesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(alternatives);

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-id", ValidResourceId);

        var result = ValidateAndDeserializeResponse(response, OptimizationJsonContext.Default.RecommendationAlternativesResult);
        Assert.Single(result.Alternatives);
        Assert.Contains("Standard_D2s_v5", result.Markdown);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.GetAlternativesAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Test error"));

        var response = await ExecuteCommandAsync("--subscription", "sub123", "--resource-id", ValidResourceId);

        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
    }
}
