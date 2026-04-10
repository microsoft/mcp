// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.UnitTests;

public class AccessTokenHandlerTests
{
    private readonly IAzureTokenCredentialProvider _mockTokenCredentialProvider;
    private readonly TokenCredential _mockTokenCredential;
    private readonly string[] _oauthScopes;

    public AccessTokenHandlerTests()
    {
        _mockTokenCredentialProvider = Substitute.For<IAzureTokenCredentialProvider>();
        _mockTokenCredential = Substitute.For<TokenCredential>();
        _oauthScopes = ["https://management.azure.com/.default"];

        // Setup mock token credential provider to return mock token credential
        _mockTokenCredentialProvider
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(_mockTokenCredential));

        // Setup mock token credential to return a valid access token
        _mockTokenCredential
            .GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("test-access-token", DateTimeOffset.UtcNow.AddHours(1))));
    }

    [Fact]
    public async Task SendAsync_FetchesAccessTokenUsingTokenCredentialProvider()
    {
        // Arrange
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert - Verify GetTokenCredentialAsync was called
        await _mockTokenCredentialProvider.Received(1).GetTokenCredentialAsync(
            null,
            Arg.Any<CancellationToken>());

        // Verify GetTokenAsync was called with correct scopes
        await _mockTokenCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(ctx => ctx.Scopes.SequenceEqual(_oauthScopes)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_SetsAuthorizationHeaderWithBearerToken()
    {
        // Arrange
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(mockInnerHandler.LastRequest);
        Assert.NotNull(mockInnerHandler.LastRequest.Headers.Authorization);
        Assert.Equal("Bearer", mockInnerHandler.LastRequest.Headers.Authorization.Scheme);
        Assert.Equal("test-access-token", mockInnerHandler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_OverwritesExistingAuthorizationHeader()
    {
        // Arrange
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Set an existing authorization header
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "existing-token");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(mockInnerHandler.LastRequest);
        Assert.NotNull(mockInnerHandler.LastRequest.Headers.Authorization);
        Assert.Equal("Bearer", mockInnerHandler.LastRequest.Headers.Authorization.Scheme);
        Assert.Equal("test-access-token", mockInnerHandler.LastRequest.Headers.Authorization.Parameter);
        Assert.NotEqual("existing-token", mockInnerHandler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_UsesCorrectOAuthScopes()
    {
        // Arrange
        var customScopes = new[] { "https://custom.scope/.default", "https://another.scope/.default" };
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, customScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        await _mockTokenCredential.Received(1).GetTokenAsync(
            Arg.Is<TokenRequestContext>(ctx => ctx.Scopes.SequenceEqual(customScopes)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_CallsBaseHandlerAfterSettingAuthorizationHeader()
    {
        // Arrange
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act
        var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(mockInnerHandler.WasCalled);
    }

    [Fact]
    public async Task SendAsync_WorksWithMultipleRequests()
    {
        // Arrange
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);

        // Act - Make multiple requests
        var request1 = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test1");
        var response1 = await client.SendAsync(request1, TestContext.Current.CancellationToken);

        var request2 = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/test2");
        var response2 = await client.SendAsync(request2, TestContext.Current.CancellationToken);

        // Assert - Both requests should have authorization header set
        Assert.NotNull(response1);
        Assert.NotNull(response2);

        // Verify GetTokenCredentialAsync was called twice (once per request)
        await _mockTokenCredentialProvider.Received(2).GetTokenCredentialAsync(
            null,
            Arg.Any<CancellationToken>());

        // Verify GetTokenAsync was called twice
        await _mockTokenCredential.Received(2).GetTokenAsync(
            Arg.Any<TokenRequestContext>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendAsync_PropagatesExceptionsFromTokenProvider()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Token provider failed");
        _mockTokenCredentialProvider
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<Task<TokenCredential>>(_ => throw expectedException);

        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await client.SendAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal("Token provider failed", actualException.Message);
    }

    [Fact]
    public async Task SendAsync_PropagatesExceptionsFromTokenCredential()
    {
        // Arrange
        var expectedException = new RequestFailedException("Authentication failed");
        _mockTokenCredential
            .GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns<ValueTask<AccessToken>>(_ => throw expectedException);

        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<RequestFailedException>(
            async () => await client.SendAsync(request, TestContext.Current.CancellationToken));

        Assert.Equal("Authentication failed", actualException.Message);
    }

    [Fact]
    public void Constructor_AcceptsValidParameters()
    {
        // Act
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);

        // Assert
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task SendAsync_RequestsTokenWithNullTenantId()
    {
        // Arrange
        var handler = new AccessTokenHandler(_mockTokenCredentialProvider, _oauthScopes);
        var mockInnerHandler = new MockHttpMessageHandler();
        handler.InnerHandler = mockInnerHandler;

        using var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

        // Act
        await client.SendAsync(request, TestContext.Current.CancellationToken);

        // Assert - Verify tenantId parameter is null
        await _mockTokenCredentialProvider.Received(1).GetTokenCredentialAsync(
            Arg.Is<string?>(tenantId => tenantId == null),
            Arg.Any<CancellationToken>());
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }
        public bool WasCalled { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            WasCalled = true;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success", System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
