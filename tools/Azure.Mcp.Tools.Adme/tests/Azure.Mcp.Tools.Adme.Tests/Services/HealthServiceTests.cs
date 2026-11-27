// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Adme.Services;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests.Services;

public sealed class HealthServiceTests
{
    private const string AuthAppId = "e91be4a4-1111-2222-3333-444444444444";

    [Fact]
    public async Task CheckHealthAsync_SucceedsAndSendsAuthenticationHeaders()
    {
        var azureService = CreateAzureService(TestConstants.AccessToken);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), azureService, new FakeHttpClientFactory(handler));

        var result = await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Result.StatusCode);
        Assert.Equal("/api/storage/v2/info", handler.LastRequest!.RequestUri!.AbsolutePath);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal(TestConstants.AccessToken, handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Equal(TestConstants.DataPartition, handler.LastRequest.Headers.GetValues("data-partition-id").Single());
        await azureService.Received(1).GetTokenCredentialAsync(TestConstants.Tenant, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_WithTenantDisplayName_UsesResolvedTenantCredential()
    {
        const string tenantName = "Contoso Ltd";
        const string tenantId = "22222222-2222-2222-2222-222222222222";
        var resolvedCredential = Substitute.For<TokenCredential>();
        resolvedCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(TestConstants.AccessToken, DateTimeOffset.UtcNow.AddHours(1)));
        var azureService = Substitute.For<IAzureService>();
        azureService.ResolveTenantIdAsync(tenantName, Arg.Any<CancellationToken>()).Returns(tenantId);
        azureService.GetTokenCredentialAsync(tenantId, Arg.Any<CancellationToken>()).Returns(resolvedCredential);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), azureService, new FakeHttpClientFactory(handler));

        await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            tenantName,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(TestConstants.AccessToken, handler.LastRequest!.Headers.Authorization!.Parameter);
        await azureService.Received(1).GetTokenCredentialAsync(tenantId, Arg.Any<CancellationToken>());
        await azureService.DidNotReceive().GetTokenCredentialAsync(tenantName, Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CheckHealthAsync_WithoutTenant_UsesDefaultCredentialWithoutResolvingTenant(string? tenant)
    {
        var azureService = CreateAzureService();
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), azureService, new FakeHttpClientFactory(handler));

        await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            tenant,
            cancellationToken: TestContext.Current.CancellationToken);

        await azureService.DidNotReceive().ResolveTenantIdAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await azureService.Received(1).GetTokenCredentialAsync(null, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_WithAuthAppId_RequestsInstanceScope()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(
                Arg.Is<TokenRequestContext>(context =>
                context.Scopes.SequenceEqual(new[] { $"{AuthAppId}/.default" })),
                Arg.Any<CancellationToken>())
            .Returns(new AccessToken(TestConstants.AccessToken, DateTimeOffset.UtcNow.AddHours(1)));
        var azureService = AzureServiceSubstitute.Create();
        azureService.GetTokenCredentialAsync(TestConstants.Tenant, Arg.Any<CancellationToken>()).Returns(credential);
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), azureService, new FakeHttpClientFactory(handler));

        var result = await service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            TestConstants.Tenant,
            AuthAppId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(200, result.Result.StatusCode);
        await credential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(context =>
                context.Scopes.SequenceEqual(new[] { $"{AuthAppId}/.default" })),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_WhenAuthenticationFails_DoesNotCallAdme()
    {
        var azureService = AzureServiceSubstitute.Create();
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task<TokenCredential>>(_ => throw new InvalidOperationException("no credential available"));
        var handler = new StubHttpMessageHandler(_ =>
            throw new InvalidOperationException("ADME should not be called when auth fails"));
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), azureService, new FakeHttpClientFactory(handler));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Null(handler.LastRequest);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenEndpointReturnsFailure_ThrowsAdmeError()
    {
        const string errorResponse = "Storage service unavailable.";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
        {
            Content = new StringContent(errorResponse)
        });
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), CreateAzureService(), new FakeHttpClientFactory(handler));

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(503, exception.Status);
        Assert.Contains(errorResponse, exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "ADME authentication failed: invalid token.")]
    [InlineData(HttpStatusCode.Forbidden, "ADME authorization failed: access denied.")]
    public async Task CheckHealthAsync_WhenAdmeRejectsAuthentication_ReportsAuthFailure(
        HttpStatusCode statusCode,
        string expectedError)
    {
        const string correlationId = "health-correlation-id";
        var handler = new StubHttpMessageHandler(_ =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(expectedError)
            };
            response.Headers.Add(AdmeServiceHelper.CorrelationIdHeader, correlationId);
            return response;
        });
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()), CreateAzureService(), new FakeHttpClientFactory(handler));

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => service.CheckHealthAsync(
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal((int)statusCode, exception.Status);
        Assert.Contains(expectedError, exception.Message);
        Assert.Contains(correlationId, exception.Message);
    }

    [Theory]
    [InlineData("https://evil.example")]
    [InlineData("https://sample.energy.azure.com.evil.example")]
    [InlineData("https://sample.oep.ppe.azure-int.net.evil.example")]
    [InlineData("http://sample.energy.azure.com")]
    [InlineData("http://sample.oep.ppe.azure-int.net")]
    public async Task CheckHealthAsync_RejectsUntrustedEndpoint(string endpoint)
    {
        var service = new HealthService(new EndpointValidator(new SsrfProtectionPolicy(null), NullLogger<EndpointValidator>.Instance, new CommandContextAccessor()),
            CreateAzureService(),
            new FakeHttpClientFactory(new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK))));

        await Assert.ThrowsAsync<System.Security.SecurityException>(() => service.CheckHealthAsync(
            endpoint,
            TestConstants.DataPartition,
            null,
            cancellationToken: TestContext.Current.CancellationToken));
    }

    private static IAzureService CreateAzureService(string token = "fake-token")
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken(token, DateTimeOffset.UtcNow.AddHours(1)));
        var azureService = AzureServiceSubstitute.Create();
        azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return azureService;
    }
}
