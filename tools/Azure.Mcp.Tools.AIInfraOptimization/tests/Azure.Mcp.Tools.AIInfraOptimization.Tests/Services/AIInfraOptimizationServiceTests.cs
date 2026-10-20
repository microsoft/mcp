// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Common;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Request;
using Azure.Mcp.Tools.AIInfraOptimization.Models.Response;
using Azure.Mcp.Tools.AIInfraOptimization.Models.VmSku;
using Azure.Mcp.Tools.AIInfraOptimization.Services;
using Azure.ResourceManager.Resources;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AIInfraOptimization.Tests.Services;

public class AIInfraOptimizationServiceTests
{
    private const string SubscriptionId = "12345678-1234-1234-1234-123456789012";

    private readonly IAzureService _azureService = Substitute.For<IAzureService>();
    private readonly IAIInfraOptimizationArmClient _armClient = Substitute.For<IAIInfraOptimizationArmClient>();
    private readonly AIInfraOptimizationService _service;
    private readonly AIInfraOptimizationRecommendVmSkuRequestBody _body = new() { SubscriptionOptions = new() };

    public AIInfraOptimizationServiceTests()
    {
        _service = new AIInfraOptimizationService(_azureService, _armClient);
        _armClient
            .RecommendVmSkuAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AIInfraOptimizationRecommendVmSkuRequestBody>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new VmSkuRecommendResult(null!, new AIInfraOptimizationArmResponseContext(null, null, null, null)));
    }

    private void ResolveSubscription(string input, string? tenant = null)
    {
        var subscriptionResource = Substitute.For<SubscriptionResource>();
        subscriptionResource.Id.Returns(SubscriptionResource.CreateResourceIdentifier(SubscriptionId));
        _azureService.GetSubscription(input, tenant, Arg.Any<CancellationToken>()).Returns(subscriptionResource);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_ResolvesCanonicalSubscriptionId()
    {
        ResolveSubscription("my-subscription-name");

        await _service.RecommendVmSkuAsync("my-subscription-name", "eastus2", _body, cancellationToken: TestContext.Current.CancellationToken);

        await _armClient.Received(1).RecommendVmSkuAsync(
            SubscriptionId,
            "eastus2",
            _body,
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecommendVmSkuAsync_PropagatesTenantAndCancellation()
    {
        ResolveSubscription(SubscriptionId, "tenant-1");
        using var cts = new CancellationTokenSource();

        await _service.RecommendVmSkuAsync(SubscriptionId, "eastus2", _body, "tenant-1", cts.Token);

        await _azureService.Received(1).GetSubscription(SubscriptionId, "tenant-1", cts.Token);
        await _armClient.Received(1).RecommendVmSkuAsync(SubscriptionId, "eastus2", _body, "tenant-1", cts.Token);
    }

    [Fact]
    public async Task RecommendVmSkuAsync_AmbiguousSubscriptionName_FailsBeforeCallingArmClient()
    {
        _azureService
            .GetSubscription("dup", Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Multiple subscriptions found"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RecommendVmSkuAsync("dup", "eastus2", _body, cancellationToken: TestContext.Current.CancellationToken));

        await DidNotCallArmClientAsync();
    }

    private async Task DidNotCallArmClientAsync() =>
        await _armClient.DidNotReceive().RecommendVmSkuAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<AIInfraOptimizationRecommendVmSkuRequestBody>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());

    [Theory]
    [InlineData("", "eastus2")]
    [InlineData(SubscriptionId, "")]
    public async Task RecommendVmSkuAsync_RequiresSubscriptionAndLocation(string subscription, string location)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RecommendVmSkuAsync(subscription, location, _body, cancellationToken: TestContext.Current.CancellationToken));

        await DidNotCallArmClientAsync();
    }

    [Fact]
    public async Task RecommendVmSkuAsync_ReturnsArmClientResult()
    {
        ResolveSubscription(SubscriptionId);
        var expected = new VmSkuRecommendResult(null!, new AIInfraOptimizationArmResponseContext("req", "client", null, "5"));
        _armClient
            .RecommendVmSkuAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<AIInfraOptimizationRecommendVmSkuRequestBody>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _service.RecommendVmSkuAsync(SubscriptionId, "eastus2", _body, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        Assert.Equal("5", result.Context.RetryAfter);
    }
}
