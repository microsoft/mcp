// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;

namespace Azure.Mcp.Tools.Functions.Services.Helpers;

/// <summary>
/// Helper for creating HTTP clients with standard configuration.
/// </summary>
internal static class HttpClientHelper
{
    private static readonly string s_version = GetVersion();

    private static readonly string s_userAgent = $"Azure-MCP-Server/{s_version}";

    /// <summary>
    /// Creates an HTTP client with the standard User-Agent header.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory.</param>
    /// <returns>A configured HTTP client.</returns>
    public static HttpClient CreateClientWithUserAgent(IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(s_userAgent);
        return client;
    }

    /// <summary>
    /// Gets the version from the entry assembly (server) or falls back to the current assembly.
    /// </summary>
    private static string GetVersion()
    {
        // Prefer entry assembly (the server) for version when running as part of the server
        var entryAssembly = Assembly.GetEntryAssembly();
        var version = entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

        if (!string.IsNullOrEmpty(version))
        {
            return version;
        }

        // Fall back to current assembly version (for unit tests or standalone usage)
        return typeof(HttpClientHelper).Assembly
            .GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
    }
}
