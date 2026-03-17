// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Functions.Services.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Functions.UnitTests.Services;

public sealed class HttpClientHelperTests
{
    /// <summary>
    /// A simple test IHttpClientFactory implementation.
    /// </summary>
    private sealed class TestHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    [Fact]
    public void CreateClientWithUserAgent_SetsUserAgentHeader()
    {
        // Arrange
        var httpClientFactory = new TestHttpClientFactory();

        // Act
        using var client = HttpClientHelper.CreateClientWithUserAgent(httpClientFactory);

        // Assert
        Assert.NotNull(client);
        var userAgent = client.DefaultRequestHeaders.UserAgent.ToString();
        Assert.StartsWith("Azure-MCP-Server/", userAgent);
    }

    [Fact]
    public void CreateClientWithUserAgent_HasValidVersion()
    {
        // Arrange
        var httpClientFactory = new TestHttpClientFactory();

        // Act
        using var client = HttpClientHelper.CreateClientWithUserAgent(httpClientFactory);

        // Assert
        var userAgent = client.DefaultRequestHeaders.UserAgent.ToString();

        // Version should not be "unknown" - assembly should have a version
        Assert.DoesNotContain("unknown", userAgent);

        // Version should be a valid semver-like format (e.g., "1.0.0.0" or "2.0.0-beta.28")
        // In unit tests, falls back to toolset version; in server, uses server version
        var version = userAgent.Replace("Azure-MCP-Server/", "");
        Assert.Matches(@"^\d+\.\d+", version); // At least major.minor
    }

    [Fact]
    public void CreateClientWithUserAgent_ReturnsSameVersionAcrossCalls()
    {
        // Arrange
        var httpClientFactory = new TestHttpClientFactory();

        // Act
        using var client1 = HttpClientHelper.CreateClientWithUserAgent(httpClientFactory);
        using var client2 = HttpClientHelper.CreateClientWithUserAgent(httpClientFactory);

        // Assert - version should be consistent (static field)
        var userAgent1 = client1.DefaultRequestHeaders.UserAgent.ToString();
        var userAgent2 = client2.DefaultRequestHeaders.UserAgent.ToString();
        Assert.Equal(userAgent1, userAgent2);
    }
}
