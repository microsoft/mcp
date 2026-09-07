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
    [InlineData(TestConstants.Endpoint)]
    [InlineData("https://sample.oep.ppe.azure-int.net")]
    public void ValidateEndpoint_AcceptsTrustedEndpoint(string endpoint)
    {
        var result = AdmeServiceHelper.ValidateEndpoint(new Uri(endpoint));

        Assert.Equal(endpoint, result.AbsoluteUri.TrimEnd('/'));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "ADME rejected the client request")]
    [InlineData(HttpStatusCode.Unauthorized, "ADME authentication failed")]
    [InlineData(HttpStatusCode.Forbidden, "ADME authorization failed")]
    public async Task SendAsync_MapsAdmeFailureStatusAndMessage(
        HttpStatusCode statusCode,
        string expectedMessage)
    {
        var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent("sensitive backend details"),
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
        Assert.DoesNotContain("sensitive backend details", exception.Message);
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
