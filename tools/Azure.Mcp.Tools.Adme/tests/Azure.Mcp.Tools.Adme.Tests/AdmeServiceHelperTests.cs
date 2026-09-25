// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure;
using Azure.Core;
using Azure.Mcp.Tools.Adme.Commands;
using Azure.Mcp.Tools.Adme.Tests.TestSupport;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Adme.Tests;

public sealed class AdmeServiceHelperTests
{
    [Theory]
    [InlineData(null, AdmeServiceHelper.AuthScope)]
    [InlineData(" ", AdmeServiceHelper.AuthScope)]
    [InlineData("e91be4a4-1111-2222-3333-444444444444", "e91be4a4-1111-2222-3333-444444444444/.default")]
    [InlineData("api://e91be4a4-1111-2222-3333-444444444444", "api://e91be4a4-1111-2222-3333-444444444444/.default")]
    [InlineData("api://e91be4a4-1111-2222-3333-444444444444/.default", "api://e91be4a4-1111-2222-3333-444444444444/.default")]
    [InlineData("https://energy.contoso.com/", "https://energy.contoso.com/.default")]
    public void GetAuthScope_ReturnsExpectedScope(string? authAppId, string expected)
    {
        Assert.Equal(expected, AdmeServiceHelper.GetAuthScope(authAppId));
    }

    [Theory]
    [InlineData("not-an-app-id")]
    [InlineData("http://energy.contoso.com")]
    [InlineData("api://app?query=value")]
    public void GetAuthScope_RejectsInvalidApplicationId(string authAppId)
    {
        Assert.Throws<ArgumentException>(() => AdmeServiceHelper.GetAuthScope(authAppId));
    }

    [Fact]
    public async Task SendAsync_PreservesAdmeFailureResponse()
    {
        const string responseContent = "{\"code\":400,\"message\":\"Invalid cursor\"}";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(responseContent),
        });

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => AdmeServiceHelper.SendAsync(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(handler),
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            "/api/test",
            AdmeJsonContext.Default.JsonElement,
            TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.BadRequest, exception.Status);
        Assert.Equal(responseContent, exception.Message);
    }

    [Fact]
    public async Task SendAsync_AppendsDataPartitionHintToUnauthorizedResponse()
    {
        const string responseContent = "User is unauthorized to perform this action";
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(responseContent),
        });

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => AdmeServiceHelper.SendAsync(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(handler),
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            "/api/test",
            AdmeJsonContext.Default.JsonElement,
            TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.Unauthorized, exception.Status);
        Assert.StartsWith(responseContent, exception.Message);
        Assert.Contains("verify the data partition name is correct and correctly cased", exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "ADME rejected the client request")]
    [InlineData(HttpStatusCode.Unauthorized, "ADME authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, "ADME authorization failed")]
    public async Task SendAsync_UsesFallbackMessageForEmptyFailureResponse(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("  "),
        });

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => AdmeServiceHelper.SendAsync(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(handler),
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            "/api/test",
            AdmeJsonContext.Default.JsonElement,
            TestContext.Current.CancellationToken));

        Assert.Equal((int)statusCode, exception.Status);
        Assert.StartsWith(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task SendAsync_TruncatesLongAdmeFailureResponse()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(new string('a', 2000)),
        });

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => AdmeServiceHelper.SendAsync(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(handler),
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            "/api/test",
            AdmeJsonContext.Default.JsonElement,
            TestContext.Current.CancellationToken));

        Assert.Equal(new string('a', 1024), exception.Message);
    }

    [Fact]
    public async Task SendAsync_ThrowsRequestFailedExceptionForNullResponseBody()
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json"),
        });

        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => AdmeServiceHelper.SendAsync(
            CreateCredentialProvider(),
            new FakeHttpClientFactory(handler),
            TestConstants.Endpoint,
            TestConstants.DataPartition,
            null,
            "/api/test",
            AdmeJsonContext.Default.SchemaListResponse,
            TestContext.Current.CancellationToken));

        Assert.Equal((int)HttpStatusCode.OK, exception.Status);
        Assert.Contains("empty response body", exception.Message);
    }

    private static IAzureTokenCredentialProvider CreateCredentialProvider()
    {
        var credential = Substitute.For<TokenCredential>();
        credential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTimeOffset.UtcNow.AddHours(1)));
        var provider = Substitute.For<IAzureTokenCredentialProvider>();
        provider.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(credential);
        return provider;
    }
}
