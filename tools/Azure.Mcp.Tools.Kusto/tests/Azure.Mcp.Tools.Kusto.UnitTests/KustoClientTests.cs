// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Http;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class KustoClientTests
{
    private readonly TokenCredential _tokenCredential;
    private readonly IHttpClientService _httpClientService;

    public KustoClientTests()
    {
        _tokenCredential = Substitute.For<TokenCredential>();
        _tokenCredential.GetTokenAsync(Arg.Any<TokenRequestContext>(), Arg.Any<CancellationToken>())
            .Returns(new ValueTask<AccessToken>(new AccessToken("noop-token", DateTimeOffset.UtcNow.AddHours(1))));

        var httpClientOptions = new HttpClientOptions
        {
            DefaultTimeout = TimeSpan.FromSeconds(100)
        };
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(httpClientOptions);
        _httpClientService = new HttpClientService(optionsWrapper);
    }

    [Fact]
    public void Constructor_SetsTimeoutTo240Seconds()
    {
        // Arrange
        var clusterUri = "https://test.kusto.windows.net";

        // Act
        var kustoClient = new KustoClient(clusterUri, _tokenCredential, "TestAgent", _httpClientService);

        // Assert
        // Use reflection to access the private _httpClient field
        var httpClientField = typeof(KustoClient).GetField("_httpClient", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(httpClientField);
        var httpClient = httpClientField.GetValue(kustoClient) as HttpClient;
        Assert.NotNull(httpClient);
        Assert.Equal(TimeSpan.FromSeconds(240), httpClient.Timeout);
    }

    #region SSRF Protection Tests

    [Theory]
    [InlineData("https://example.com/v1/rest/query")]
    [InlineData("https://external-server.com")]
    [InlineData("http://test.kusto.windows.net")] // HTTP instead of HTTPS
    [InlineData("https://kusto.windows.net.example.com")]
    [InlineData("https://test.kusto.windows.net.example.com/path")]
    [InlineData("https://169.254.169.254/metadata/instance")] // Azure IMDS
    [InlineData("https://management.azure.com/subscriptions")]
    public void Constructor_RejectsInvalidClusterUris(string invalidClusterUri)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new KustoClient(invalidClusterUri, _tokenCredential, "azmcp", _httpClientService));

        Assert.Contains("Kusto cluster URI", exception.Message);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://test.kusto.windows.net")]
    [InlineData("file:///etc/passwd")]
    public void Constructor_RejectsNonHttpsSchemes(string invalidClusterUri)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new KustoClient(invalidClusterUri, _tokenCredential, "azmcp", _httpClientService));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Constructor_RejectsNullOrEmptyClusterUri(string? invalidClusterUri)
    {
        // Act & Assert - ArgumentNullException is thrown for null, ArgumentException for empty/whitespace
        Assert.ThrowsAny<ArgumentException>(
            () => new KustoClient(invalidClusterUri!, _tokenCredential, "azmcp", _httpClientService));
    }

    [Theory]
    [InlineData("https://mycluster.kusto.windows.net")]
    [InlineData("https://mycluster.westus.kusto.windows.net")]
    [InlineData("https://mycluster.eastus2.kusto.windows.net/")]
    [InlineData("https://test-cluster.northeurope.kusto.windows.net")]
    // China cloud
    [InlineData("https://mycluster.kusto.chinacloudapi.cn")]
    [InlineData("https://mycluster.eastus.kusto.chinacloudapi.cn")]
    // US Government
    [InlineData("https://mycluster.kusto.usgovcloudapi.net")]
    [InlineData("https://mycluster.usgovvirginia.kusto.usgovcloudapi.net")]
    // Dev
    [InlineData("https://mycluster.kustodev.windows.net")]
    // Fabric & Synapse
    [InlineData("https://mycluster.kusto.fabric.microsoft.com")]
    [InlineData("https://mycluster.kusto.azuresynapse.net")]
    // Log Analytics / App Insights
    [InlineData("https://mycluster.adx.loganalytics.azure.com")]
    [InlineData("https://mycluster.adx.applicationinsights.azure.com")]
    // Germany
    [InlineData("https://mycluster.kusto.sovcloud-api.de")]
    public void Constructor_AcceptsValidKustoClusterUris(string validClusterUri)
    {
        // Act - should not throw
        var client = new KustoClient(validClusterUri, _tokenCredential, "azmcp", _httpClientService);

        // Assert - client was created successfully (no exception thrown)
        Assert.NotNull(client);
    }

    [Theory]
    // Public cloud exact hostnames
    [InlineData("https://kusto.aria.microsoft.com")]
    [InlineData("https://eu.kusto.aria.microsoft.com")]
    [InlineData("https://ade.applicationinsights.io")]
    [InlineData("https://ade.loganalytics.io")]
    [InlineData("https://adx.aimon.applicationinsights.azure.com")]
    [InlineData("https://adx.applicationinsights.azure.com")]
    [InlineData("https://adx.loganalytics.azure.com")]
    [InlineData("https://adx.monitor.azure.com")]
    // US Government exact hostnames
    [InlineData("https://adx.applicationinsights.azure.us")]
    [InlineData("https://adx.loganalytics.azure.us")]
    [InlineData("https://adx.monitor.azure.us")]
    // China exact hostnames
    [InlineData("https://adx.applicationinsights.azure.cn")]
    [InlineData("https://adx.loganalytics.azure.cn")]
    [InlineData("https://adx.monitor.azure.cn")]
    // Germany
    [InlineData("https://adx.applicationinsights.azure.de")]
    [InlineData("https://adx.loganalytics.azure.de")]
    [InlineData("https://adx.monitor.azure.de")]
    public void Constructor_AcceptsValidKustoExactHostnames(string validClusterUri)
    {
        // Act - should not throw
        var client = new KustoClient(validClusterUri, _tokenCredential, "azmcp", _httpClientService);

        // Assert - client was created successfully (no exception thrown)
        Assert.NotNull(client);
    }

    [Theory]
    // Single segment cluster names
    [InlineData("https://mycluster.kusto.windows.net")]
    [InlineData("https://a.kusto.windows.net")] // Single character
    [InlineData("https://cluster123.kusto.windows.net")] // With numbers
    [InlineData("https://my-cluster.kusto.windows.net")] // With hyphen
    [InlineData("https://my-cluster-name.kusto.windows.net")] // Multiple hyphens
    // Two segment cluster names (cluster.region)
    [InlineData("https://mycluster.westus.kusto.windows.net")]
    [InlineData("https://mycluster.eastus2.kusto.windows.net")]
    [InlineData("https://my-cluster.north-europe.kusto.windows.net")]
    // Three or more segment cluster names
    [InlineData("https://mycluster.sub1.sub2.kusto.windows.net")]
    [InlineData("https://a.b.c.d.kusto.windows.net")]
    [InlineData("https://cluster-1.region-2.zone-3.kusto.windows.net")]
    public void Constructor_AcceptsMultiSegmentClusterNames(string validClusterUri)
    {
        // Act - should not throw
        var client = new KustoClient(validClusterUri, _tokenCredential, "azmcp", _httpClientService);

        // Assert - client was created successfully (no exception thrown)
        Assert.NotNull(client);
    }

    [Theory]
    // Empty cluster name (just the suffix)
    [InlineData("https://.kusto.windows.net")]
    // Segment starting with hyphen
    [InlineData("https://-mycluster.kusto.windows.net")]
    [InlineData("https://mycluster.-region.kusto.windows.net")]
    // Empty segment (double dots)
    [InlineData("https://mycluster..kusto.windows.net")]
    [InlineData("https://mycluster.region..kusto.windows.net")]
    // Segment with invalid characters
    [InlineData("https://my_cluster.kusto.windows.net")] // Underscore
    [InlineData("https://my!cluster.kusto.windows.net")] // Exclamation
    // Segment with only hyphens (must start with alphanumeric)
    [InlineData("https://---.kusto.windows.net")]
    public void Constructor_RejectsInvalidClusterNameSegments(string invalidClusterUri)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new KustoClient(invalidClusterUri, _tokenCredential, "azmcp", _httpClientService));
    }

    #endregion
}
