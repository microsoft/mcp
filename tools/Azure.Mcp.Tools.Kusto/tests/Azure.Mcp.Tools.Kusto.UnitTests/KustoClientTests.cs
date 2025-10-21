// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Kusto.Services;
using Azure.Core;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class KustoClientTests
{
    [Fact]
    public void Constructor_SetsTimeoutTo240Seconds()
    {
        // Arrange
        var clusterUri = "https://test.kusto.windows.net";
        var tokenCredential = Substitute.For<TokenCredential>();
        var userAgent = "TestAgent";
        var httpClientOptions = new HttpClientOptions
        {
            DefaultTimeout = TimeSpan.FromSeconds(100)
        };
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(httpClientOptions);
        var httpClientService = new HttpClientService(optionsWrapper);

        // Act
        var kustoClient = new KustoClient(clusterUri, tokenCredential, userAgent, httpClientService);

        // Assert
        // Use reflection to access the private _httpClient field
        var httpClientField = typeof(KustoClient).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(httpClientField);
        var httpClient = httpClientField.GetValue(kustoClient) as HttpClient;
        Assert.NotNull(httpClient);
        Assert.Equal(TimeSpan.FromSeconds(240), httpClient.Timeout);
    }
}
