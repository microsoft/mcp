// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Security;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Helpers;
using Azure.Mcp.Tools.ServiceBus.Services;
using Azure.Mcp.Tools.ServiceBus.Tests.TestSupport;
using Azure.Messaging.ServiceBus;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ServiceBus.Tests.Services;

public class ServiceBusServiceNamespaceValidationTests
{
    private readonly IAzureService _azureService = AzureServiceTestHelpers.CreateAzureService();
    private readonly ServiceBusService _service;

    public ServiceBusServiceNamespaceValidationTests()
    {
        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        _azureService.CloudConfiguration.Returns(cloudConfig);
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<TokenCredential>());
        _azureService.GetClient().Returns(_ => new HttpClient(new HttpClientHandler()));

        _service = new ServiceBusService(_azureService);
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    [InlineData("mynamespace.servicebus.windows.net.evil.com")]
    public async Task GetQueueDetails_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetQueueDetails(namespaceName, "testQueue", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task GetTopicDetails_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetTopicDetails(namespaceName, "testTopic", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task GetSubscriptionDetails_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.GetSubscriptionDetails(namespaceName, "testTopic", "testSub", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task PeekQueueMessages_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.PeekQueueMessages(namespaceName, "testQueue", 1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("attacker.dssldrf.net")]
    [InlineData("evil.com")]
    public async Task PeekSubscriptionMessages_RejectsAttackerControlledNamespace(string namespaceName)
    {
        var ex = await Assert.ThrowsAsync<SecurityException>(
            () => _service.PeekSubscriptionMessages(namespaceName, "testTopic", "testSub", 1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("not a valid servicebus domain", ex.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ns#fragment.servicebus.windows.net")]
    [InlineData("test.servicebus.windows.net#evil.com")]
    [InlineData("test.servicebus.windows.net#")]
    [InlineData("test.servicebus.windows.net/")]
    [InlineData("test.servicebus.windows.net/extraPath")]
    [InlineData("test.servicebus.windows.net:443")]
    [InlineData("test.servicebus.windows.net?")]
    [InlineData("test.servicebus.windows.net?q=1")]
    [InlineData("https://test.servicebus.windows.net")]
    [InlineData("test.servicebus.windows.net\\evil.example")]
    [InlineData("test.servicebus.windows.net@evil.example")]
    [InlineData(" test.servicebus.windows.net")]
    [InlineData("test.servicebus.windows.net ")]
    [InlineData("test.servicebus.windows.net\t")]
    [InlineData("test.servicebus.windows.net\r\n")]
    [InlineData("test.servicebus.windows.net\0")]
    [InlineData("test..servicebus.windows.net")]
    [InlineData("test,other.servicebus.windows.net")]
    [InlineData("test%2Fother.servicebus.windows.net")] // cspell:disable-line
    [InlineData("10.0.0.1")]
    [InlineData("::1")]
    [InlineData("[::1]")]
    public async Task GetQueueDetails_RejectsMalformedNamespace(string namespaceName)
    {
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetQueueDetails(namespaceName, "testQueue", cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Namespace name must be a bare DNS hostname", ex.Message);
        Assert.Contains(namespaceName, ex.Message);
        Assert.Equal("namespaceName", ex.ParamName);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("ns#fragment.servicebus.windows.net")]
    [InlineData("test.servicebus.windows.net/extraPath")]
    [InlineData("test.servicebus.windows.net?q=1")]
    [InlineData("test.servicebus.windows.net:443")]
    [InlineData("test.servicebus.windows.net\\evil.example")]
    [InlineData("10.0.0.1")]
    public async Task PeekQueueMessages_RejectsMalformedNamespace(string namespaceName)
    {
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(
            () => _service.PeekQueueMessages(namespaceName, "testQueue", 1, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Contains("Namespace name must be a bare DNS hostname", ex.Message);
        Assert.Contains(namespaceName, ex.Message);
        Assert.Equal("namespaceName", ex.ParamName);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("Public", "MY-NAMESPACE.servicebus.windows.net", "my-namespace.servicebus.windows.net")]
    [InlineData("China", "my-namespace.servicebus.chinacloudapi.cn", "my-namespace.servicebus.chinacloudapi.cn")]
    [InlineData("Government", "my-namespace.servicebus.usgovcloudapi.net", "my-namespace.servicebus.usgovcloudapi.net")]
    public async Task GetQueueDetails_ConfiguredCloud_UsesValidatedHost(
        string cloud,
        string namespaceName,
        string expectedHost)
    {
        _azureService.CloudConfiguration.ArmEnvironment.Returns(GetArmEnvironment(cloud));
        TokenCredential credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("test-token", DateTimeOffset.UtcNow.AddHours(1)));
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(credential);
        var handler = new RecordingHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        using var client = new HttpClient(handler);
        _azureService.GetClient().Returns(client);

        ServiceBusException exception = await Assert.ThrowsAsync<ServiceBusException>(() =>
            _service.GetQueueDetails(namespaceName, "testQueue", cancellationToken: TestContext.Current.CancellationToken));

        //Assert.Equal(404, exception.);
        Uri requestUri = Assert.Single(handler.RequestUris);
        Assert.Equal(Uri.UriSchemeHttps, requestUri.Scheme);
        Assert.Equal(expectedHost, requestUri.IdnHost);
    }

    [Theory]
    [InlineData("Public", "my-namespace.servicebus.chinacloudapi.cn")]
    [InlineData("Public", "my-namespace.servicebus.usgovcloudapi.net")]
    [InlineData("China", "my-namespace.servicebus.windows.net")]
    [InlineData("Government", "my-namespace.servicebus.windows.net")]
    public async Task GetQueueDetails_CrossCloudNamespace_RejectsBeforeCredentialAcquisition(
        string cloud,
        string namespaceName)
    {
        _azureService.CloudConfiguration.ArmEnvironment.Returns(GetArmEnvironment(cloud));

        await Assert.ThrowsAsync<SecurityException>(() =>
            _service.GetQueueDetails(namespaceName, "testQueue", cancellationToken: TestContext.Current.CancellationToken));

        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    private static ArmEnvironment GetArmEnvironment(string cloud) => cloud switch
    {
        "Public" => ArmEnvironment.AzurePublicCloud,
        "China" => ArmEnvironment.AzureChina,
        "Government" => ArmEnvironment.AzureGovernment,
        _ => throw new ArgumentOutOfRangeException(nameof(cloud))
    };
}
