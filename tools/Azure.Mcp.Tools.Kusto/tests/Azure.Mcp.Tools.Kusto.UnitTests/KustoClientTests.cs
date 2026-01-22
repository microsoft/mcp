// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Core;
using Azure.Mcp.Tools.Kusto.Services;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class KustoClientTests
{
    [Fact]
    public async Task ExecuteCommandAsync_SetsTimeoutTo240Seconds()
    {
        // Arrange
        var tokenCredential = Substitute.For<TokenCredential>();
        tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("noop-token", DateTimeOffset.UtcNow.AddHours(1))));

        using var httpClient = new HttpClient(new MockHttpMessageHandler());

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>()).Returns(httpClient);

        var kustoClient = new KustoClient("https://test.kusto.windows.net", tokenCredential, "azmcp", httpClientFactory);

        // Act
        var result = await kustoClient.ExecuteQueryCommandAsync("testdb", "test query", CancellationToken.None);

        // Assert - verify the timeout was set to 240 seconds
        Assert.Equal(TimeSpan.FromSeconds(240), httpClient.Timeout);
        Assert.NotNull(result);
    }

    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // The caller (HttpClient.SendAsync) takes ownership and is responsible for disposal HttpResponseMessage.
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"Tables": []}""", System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
