// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Security;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.UnitTests.Services;

public class ServiceBusServiceNamespaceValidationTests
{
    private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
    private readonly ServiceBusService _service;

    public ServiceBusServiceNamespaceValidationTests()
    {
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        _tenantService.CloudConfiguration.Returns(cloudConfig);
        _tenantService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<TokenCredential>());
        _tenantService.GetClient().Returns(_ => new HttpClient(new HttpClientHandler()));

        _service = new ServiceBusService(_tenantService);
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    [InlineData("10.0.0.1")]
    [InlineData("mynamespace.servicebus.windows.net.evil.com")]
    public async Task GetQueueDetails_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetQueueDetails(namespaceName, "testQueue", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task GetTopicDetails_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetTopicDetails(namespaceName, "testTopic", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task GetSubscriptionDetails_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetSubscriptionDetails(namespaceName, "testTopic", "testSub", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task PeekQueueMessages_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.PeekQueueMessages(namespaceName, "testQueue", 1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task PeekSubscriptionMessages_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.PeekSubscriptionMessages(namespaceName, "testTopic", "testSub", 1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
    }

    [Theory]
    [InlineData("ns#fragment.servicebus.windows.net")]
    [InlineData("test.servicebus.windows.net#evil.com")]
    [InlineData("test.servicebus.windows.net/extraPath")]
    [InlineData("test.servicebus.windows.net:443")]
    [InlineData("test.servicebus.windows.net?q=1")]
    [InlineData("https://test.servicebus.windows.net")]
    public async Task GetQueueDetails_RejectsNonHostNamespaceValues(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetQueueDetails(namespaceName, "testQueue", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("host name only", ex.Message);
    }

    [Theory]
    [InlineData("ns#fragment.servicebus.windows.net")]
    [InlineData("test.servicebus.windows.net/extraPath")]
    [InlineData("test.servicebus.windows.net:443")]
    public async Task PeekQueueMessages_RejectsNonHostNamespaceValues(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.PeekQueueMessages(namespaceName, "testQueue", 1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("host name only", ex.Message);
    }
}
