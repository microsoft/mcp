// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.IO.Compression;
using System.Text;

namespace Azure.Mcp.Tools.Kusto.Services;

internal static class DeeplinkHelper
{
    private const int MaxUrlLength = 8000;

    private const string PublicExplorerBase = "https://dataexplorer.azure.com";
    private const string UsGovExplorerBase = "https://dataexplorer.azure.us";
    private const string ChinaExplorerBase = "https://dataexplorer.azure.cn";

    private static readonly (string Suffix, string ExplorerBase)[] s_explorerCloudMappings =
    [
        // Public cloud
        (".kusto.windows.net", PublicExplorerBase),
        (".kustodev.windows.net", PublicExplorerBase),
        (".kustomfa.windows.net", PublicExplorerBase),
        (".kusto.data.microsoft.com", PublicExplorerBase),
        (".kusto.fabric.microsoft.com", PublicExplorerBase),
        (".kusto.azuresynapse.net", PublicExplorerBase),
        // US Government
        (".kusto.usgovcloudapi.net", UsGovExplorerBase),
        (".kustomfa.usgovcloudapi.net", UsGovExplorerBase),
        // China
        (".kusto.chinacloudapi.cn", ChinaExplorerBase),
        (".kustomfa.chinacloudapi.cn", ChinaExplorerBase),
        (".kusto.azuresynapse.azure.cn", ChinaExplorerBase),
    ];

    internal static string? BuildWebExplorerUrl(string clusterUri, string database, string query)
    {
        if (!Uri.TryCreate(clusterUri, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var host = uri.Host;
        var explorerBase = GetExplorerBaseUrl(host);
        if (explorerBase is null)
        {
            return null;
        }

        var queryBytes = Encoding.UTF8.GetBytes(query);

        using var memoryStream = new MemoryStream();
        using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            gzipStream.Write(queryBytes, 0, queryBytes.Length);
        }

        var base64 = Convert.ToBase64String(memoryStream.ToArray());
        var urlEncoded = Uri.EscapeDataString(base64);

        var url = $"{explorerBase}/clusters/{host}/databases/{database}?query={urlEncoded}";

        if (url.Length > MaxUrlLength)
        {
            return null;
        }

        return url;
    }

    private static string? GetExplorerBaseUrl(string host)
    {
        foreach (var (suffix, explorerBase) in s_explorerCloudMappings)
        {
            if (host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return explorerBase;
            }
        }

        return null;
    }
}
