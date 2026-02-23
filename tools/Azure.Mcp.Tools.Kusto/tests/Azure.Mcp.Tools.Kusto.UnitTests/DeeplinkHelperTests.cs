// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Text;
using Azure.Mcp.Tools.Kusto.Services;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class DeeplinkHelperTests
{
    [Fact]
    public void BuildWebExplorerUrl_SimpleQuery_ProducesValidUrl()
    {
        // Arrange
        var clusterUri = "https://help.kusto.windows.net";
        var database = "Samples";
        var query = "StormEvents | take 10";

        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl(clusterUri, database, query);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("https://dataexplorer.azure.com/clusters/help.kusto.windows.net/databases/Samples?query=", url);

        var decodedQuery = DecodeQueryFromUrl(url);
        Assert.Equal(query, decodedQuery);
    }

    [Fact]
    public void BuildWebExplorerUrl_RegionalCluster_ProducesValidUrl()
    {
        // Arrange
        var clusterUri = "https://mycluster.westus.kusto.windows.net";
        var database = "MyDb";
        var query = "MyTable | count";

        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl(clusterUri, database, query);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("https://dataexplorer.azure.com/clusters/mycluster.westus.kusto.windows.net/databases/MyDb?query=", url);
    }

    [Fact]
    public void BuildWebExplorerUrl_UsGovCloud_UsesCorrectExplorerBase()
    {
        // Arrange
        var clusterUri = "https://mycluster.kusto.usgovcloudapi.net";
        var database = "db1";
        var query = "T | take 1";

        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl(clusterUri, database, query);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("https://dataexplorer.azure.us/clusters/mycluster.kusto.usgovcloudapi.net/databases/db1?query=", url);
    }

    [Fact]
    public void BuildWebExplorerUrl_ChinaCloud_UsesCorrectExplorerBase()
    {
        // Arrange
        var clusterUri = "https://mycluster.kusto.chinacloudapi.cn";
        var database = "db1";
        var query = "T | take 1";

        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl(clusterUri, database, query);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("https://dataexplorer.azure.cn/clusters/mycluster.kusto.chinacloudapi.cn/databases/db1?query=", url);
    }

    [Fact]
    public void BuildWebExplorerUrl_QueryExceedsMaxLength_ReturnsNull()
    {
        // Arrange - incrementing integers produce deterministic data that resists gzip compression
        var clusterUri = "https://help.kusto.windows.net";
        var database = "Samples";
        var builder = new StringBuilder();
        for (var i = 0; i < 10000; i++)
        {
            builder.Append(i.ToString("X4"));
        }
        var query = builder.ToString();

        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl(clusterUri, database, query);

        // Assert
        Assert.Null(url);
    }

    [Fact]
    public void BuildWebExplorerUrl_InvalidUri_ReturnsNull()
    {
        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl("not-a-uri", "db", "query");

        // Assert
        Assert.Null(url);
    }

    [Fact]
    public void BuildWebExplorerUrl_UnsupportedDomain_ReturnsNull()
    {
        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl("https://example.com", "db", "query");

        // Assert
        Assert.Null(url);
    }

    [Fact]
    public void BuildWebExplorerUrl_TrailingSlash_ProducesValidUrl()
    {
        // Arrange
        var clusterUri = "https://help.kusto.windows.net/";
        var database = "Samples";
        var query = "StormEvents | take 10";

        // Act
        var url = DeeplinkHelper.BuildWebExplorerUrl(clusterUri, database, query);

        // Assert
        Assert.NotNull(url);
        Assert.StartsWith("https://dataexplorer.azure.com/clusters/help.kusto.windows.net/databases/Samples?query=", url);
    }

    private static string DecodeQueryFromUrl(string url)
    {
        var queryParam = new Uri(url).Query;
        var urlEncoded = queryParam["?query=".Length..];
        var base64 = Uri.UnescapeDataString(urlEncoded);
        var compressed = Convert.FromBase64String(base64);

        using var memoryStream = new MemoryStream(compressed);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
