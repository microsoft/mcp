// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Nodes;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Insights.Commands;
using Azure.Mcp.Tools.Insights.Services;
using Azure.Mcp.Tools.Insights.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
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
        var aggregation = CreatePopulatedAggregation();
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(aggregation);
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1", "--query", "build a finance app");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateSubscriptionAsync(
            "sub1",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<IProgress<string>?>(),
            Arg.Any<bool>());
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
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--tenant", "tenant-1", "--scope", "tenant");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateTenantAsync(
            "tenant-1",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<IProgress<string>?>(),
            Arg.Any<bool>());
        await Service.DidNotReceiveWithAnyArgs().AggregateSubscriptionAsync(
            default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_SamplingReturnsInsights_ReturnsResultsList()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
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
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--subscription", "\"sub1\"");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateSubscriptionAsync(
            "sub1",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<IProgress<string>?>(),
            Arg.Any<bool>());
    }

    [Theory]
    [InlineData("bogus")]
    [InlineData("Subscriptions")]
    [InlineData("")]
    public async Task ExecuteAsync_InvalidScope_ReturnsBadRequest(string scope)
    {
        var response = await ExecuteWithSamplingAsync("--scope", scope);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("scope", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().AggregateSubscriptionAsync(
            default!, default, default, TestContext.Current.CancellationToken);
        await Service.DidNotReceiveWithAnyArgs().AggregateTenantAsync(
            default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_TenantScopeWithSubscription_ReturnsBadRequest()
    {
        var response = await ExecuteWithSamplingAsync("--scope", "tenant", "--subscription", "sub1");

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("subscription", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().AggregateTenantAsync(
            default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_SubscriptionScopeWithNoDefault_ReturnsBadRequest()
    {
        _subscriptionService.GetDefaultSubscriptionId().Returns((string?)null);

        var response = await ExecuteWithSamplingAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("default subscription", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().AggregateSubscriptionAsync(
            default!, default, default, TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task ExecuteAsync_UsesDefaultSubscription_WhenNotProvided()
    {
        _subscriptionService.GetDefaultSubscriptionId().Returns("default-sub");
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync();

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateSubscriptionAsync(
            "default-sub",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<IProgress<string>?>(),
            Arg.Any<bool>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExecuteAsync_NoCacheFlag_PropagatesToService(bool noCache)
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var args = noCache
            ? new[] { "--subscription", "sub1", "--nocache" }
            : new[] { "--subscription", "sub1" };
        var response = await ExecuteWithSamplingAsync(args);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).AggregateSubscriptionAsync(
            "sub1",
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<IProgress<string>?>(),
            noCache);
    }

    [Fact]
    public async Task ExecuteAsync_QueryExceedsMaxLength_ReturnsBadRequest()
    {
        var longQuery = new string('a', 1001);

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1", "--query", longQuery);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("query", response.Message, StringComparison.OrdinalIgnoreCase);
        await Service.DidNotReceiveWithAnyArgs().AggregateSubscriptionAsync(
            default!, default, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("build   a\tfinance\u0000app", "build a finance app")]
    [InlineData("line one\n\nline two", "line one line two")]
    [InlineData("  trim me  ", "trim me")]
    public async Task ExecuteAsync_SanitizesQueryBeforeSampling(string rawQuery, string expected)
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1", "--query", rawQuery);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _samplingService.Received(1).SampleTextAsync(
            Arg.Any<McpServer>(),
            Arg.Any<string>(),
            Arg.Is<string>(payload => payload.Contains(expected)),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhitespaceOnlyQuery_OmittedFromPayload()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("[]");

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1", "--query", "  \t  ");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _samplingService.Received(1).SampleTextAsync(
            Arg.Any<McpServer>(),
            Arg.Any<string>(),
            Arg.Is<string>(payload => !payload.Contains("userQuery")),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_SamplingReturnsSensitiveContent_DropsThoseInsights()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs("""
                [
                  { "id": "insight-001", "pattern": "TLS1_2 dominates", "implication": "Pin TLS1_2." },
                  { "id": "insight-002", "pattern": "Storage uses AccountKey=abc123==", "implication": "Rotate keys." },
                  { "id": "insight-003", "pattern": "Connection found", "implication": "Server=tcp:db;Password=p@ss;" }
                ]
                """);

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1");

        var result = ValidateAndDeserializeResponse(response, InsightsJsonContext.Default.InsightsGetCommandResult);
        Assert.Single(result.Insights);
        Assert.Equal("insight-001", result.Insights[0].Id);
    }

    [Fact]
    public async Task ExecuteAsync_NoResourcesInScope_ReturnsEmptyWithoutSampling()
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreateEmptyAggregation());

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1");

        var result = ValidateAndDeserializeResponse(response, InsightsJsonContext.Default.InsightsGetCommandResult);
        Assert.Empty(result.Insights);
        await _samplingService.DidNotReceiveWithAnyArgs().SampleTextAsync(
            default!, default!, default!, default, TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData("I cannot help with that request.")]                    // model refusal (prose)
    [InlineData("Sorry, I'm unable to comply with these instructions.")] // model refusal (prose)
    [InlineData("not json at all { broken")]                            // malformed JSON
    [InlineData("")]                                                    // empty response
    [InlineData("   ")]                                                 // whitespace-only response
    [InlineData("""{ "message": "no insights here" }""")]               // valid JSON but not an insights array
    public async Task ExecuteAsync_SamplingReturnsUnparseable_ReturnsBadGatewayWithoutStackTrace(string sampled)
    {
        Service.AggregateSubscriptionAsync(default!, default, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(CreatePopulatedAggregation());
        _samplingService.SampleTextAsync(default!, default!, default!, default, TestContext.Current.CancellationToken)
            .ReturnsForAnyArgs(sampled);

        var response = await ExecuteWithSamplingAsync("--subscription", "sub1");

        Assert.Equal(HttpStatusCode.BadGateway, response.Status);
        Assert.False(string.IsNullOrWhiteSpace(response.Message));
        Assert.DoesNotContain("StackTrace", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Exception", response.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(response.Results);
    }

    private Task<CommandResponse> ExecuteWithSamplingAsync(params string[] args)
    {
        var server = Substitute.For<McpServer>();
        server.ClientCapabilities.Returns(new ClientCapabilities { Sampling = new SamplingCapability() });
        var context = new CommandContext(ServiceProvider) { McpServer = server };
        return ((IBaseCommand)Command).ExecuteAsync(context, CommandDefinition.Parse(args), TestContext.Current.CancellationToken);
    }

    private static SubscriptionAggregation CreatePopulatedAggregation() =>
        new(
            new Dictionary<string, ResourceTypeAggregation>
            {
                ["microsoft.compute/virtualmachines"] = new("microsoft.compute/virtualmachines", TotalCount: 1, new JsonObject()),
            },
            SubscriptionCount: 1,
            ResourceGroupCount: 1);

    private static SubscriptionAggregation CreateEmptyAggregation() =>
        new(new Dictionary<string, ResourceTypeAggregation>(), SubscriptionCount: 1, ResourceGroupCount: 0);
}

