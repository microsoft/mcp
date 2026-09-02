// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Services;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Services;

public sealed class HealthServiceTests
{
    [Fact]
    public async Task CheckHealthAsync_SucceedsAndSendsAuthenticationHeaders()
    {
        var provider = CreateCredentialProvider(TestConstants.AccessToken);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new HealthService(provider, new FakeHttpClientFactory(handler));

        var result = await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
            TestContext.Current.CancellationToken);

        Assert.True(result.AuthOk);
        Assert.True(result.ConnectivityOk);
        Assert.Equal(200, result.ConnectivityStatusCode);
        Assert.Equal("/api/storage/v2/info", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal(TestConstants.AccessToken, handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Equal(TestConstants.DataPartition, handler.LastRequest.Headers.GetValues("data-partition-id").Single());
        await provider.Received(1).GetTokenCredentialAsync(TestConstants.Tenant, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_WhenAuthenticationFails_DoesNotCallAdme()
    {
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task<TokenCredential>>(_ => throw new InvalidOperationException("no credential available"));
        var handler = new StubHttpMessageHandler(_ =>
            throw new InvalidOperationException("ADME should not be called when auth fails"));
        var service = new HealthService(provider, new FakeHttpClientFactory(handler));

        var result = await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            TestContext.Current.CancellationToken);

        Assert.False(result.AuthOk);
        Assert.Equal(
            "Microsoft Entra authentication failed. Verify your credentials and sign-in configuration.",
            result.AuthError);
        Assert.DoesNotContain("no credential available", result.AuthError);
        Assert.False(result.ConnectivityOk);
        Assert.Equal("Connectivity check skipped because authentication failed.", result.ConnectivityError);
        Assert.Null(result.ConnectivityStatusCode);
        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEndpointIsUnavailable_ReportsConnectivityFailure()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var service = new HealthService(CreateCredentialProvider(), new FakeHttpClientFactory(handler));

        var result = await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            TestContext.Current.CancellationToken);

        Assert.True(result.AuthOk);
        Assert.False(result.ConnectivityOk);
        Assert.Equal(503, result.ConnectivityStatusCode);
        Assert.Contains("503", result.ConnectivityError);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://sample.energy.azure.com.evil.example")]
    [InlineData("https://sample.oep.ppe.azure-int.net.evil.example")]
    [InlineData("http://sample.energy.azure.com")]
    [InlineData("http://sample.oep.ppe.azure-int.net")]
    public async Task CheckHealthAsync_RejectsUntrustedEndpoint(string endpoint)
    {
        var service = new HealthService(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))));

        await Assert.ThrowsAsync<System.Security.SecurityException>(() => service.CheckHealthAsync(
            endpoint,
            TestConstants.DataPartition,
            null,
            TestContext.Current.CancellationToken));
    }

    private static IAzureTokenCredentialProvider CreateCredentialProvider(string token = "fake-token")
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return provider;
    }
}
