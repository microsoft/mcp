// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Insights.Commands;
using Azure.Mcp.Tools.Insights.Services;
using Azure.Mcp.Tools.Insights.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Insights.UnitTests;

public class InsightsGetCommandTests : CommandUnitTestsBase<InsightsGetCommand, IInsightsService>
{
    private readonly ISamplingService _samplingService = Substitute.For<ISamplingService>();
    private readonly ISubscriptionService _subscriptionService = Substitute.For<ISubscriptionService>();

    public InsightsGetCommandTests()
    {
        Services.AddSingleton(_samplingService);
        Services.AddSingleton(_subscriptionService);
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("get", CommandDefinition.Name);
        Assert.NotEmpty(CommandDefinition.Description!);
        Assert.True(Command.Metadata.ReadOnly);
        Assert.True(Command.Metadata.Idempotent);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSamplingCapability_ReturnsBadRequest()
    {
        var response = await ExecuteCommandAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("sampling", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_WithSubscription_CallsAggregateSubscription()
    {
        var aggregation = CreateEmptyAggregation();
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(aggregation);
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1", "--query", "build a finance app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateSubscriptionAsync(
            "sub1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
        await _samplingService.Received(1).SampleTextAsync(
            Arg.Any<McpServer>(),
            Arg.Any<string>(),
            Arg.Is<string>(payload => payload.Contains("build a finance app")),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutSubscription_CallsAggregateTenant()
    {
        Service.AggregateTenantAsync(default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreateEmptyAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--tenant", "tenant-1", "--scope", "tenant");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateTenantAsync(
            "tenant-1", Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
        await Service.DidNotReceiveWithAnyArgs().AggregateSubscriptionAsync(
            default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_SamplingReturnsInsights_ReturnsResultsList()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreateEmptyAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("""
                ```json
                [
                  { "id": "insight-001", "pattern": "TLS1_2 dominates", "implication": "Pin TLS1_2." },
                  { "id": "insight-002", "pattern": "eastus dominates", "implication": "Default to eastus." },
                  { "id": "insight-003", "pattern": "missing implication is skipped", "implication": "" }
                ]
                ```
                """);

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1");

        var result = ValidateAndDeserializeResponse(response, InsightsJsonContext.Default.InsightsGetCommandResult);
        Assert.Equal(2, result.Insights.Count);
        Assert.Equal("insight-001", result.Insights[0].Id);
        Assert.Equal("insight-002", result.Insights[1].Id);
    }

    [Fact]
    public async Task ExecuteAsync_AggregateThrows_ReturnsErrorInResponse()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException("boom"));

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_QuotedSubscription_StripsQuotes()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreateEmptyAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--subscription", "\"sub1\"");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateSubscriptionAsync(
            "sub1", Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    private Task<CommandResponse> ExecuteWithSamplingAsync(params string[] args)
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Sampling = new SamplingCapability() });
        var context = new CommandContext(ServiceProvider) { McpServer = server };
        return Command.ExecuteAsync(context, CommandDefinition.Parse(args), TestContext.Current.CancellationToken);
    }

    private static SubscriptionAggregation CreateEmptyAggregation() =>
        new(new Dictionary<string, ResourceTypeAggregation>(), SubscriptionCount: 1, ResourceGroupCount: 0);
}

