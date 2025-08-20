// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Http;

/// <summary>
/// Configuration options for HttpClient services.
/// </summary>
public sealed class HttpClientOptions
{
    /// <summary>
    /// Gets or sets the HTTP proxy address. Can be set via HTTP_PROXY environment variable.
    /// </summary>
    public string? HttpProxy { get; set; }

    /// <summary>
    /// Gets or sets the HTTPS proxy address. Can be set via HTTPS_PROXY environment variable.
    /// </summary>
    public string? HttpsProxy { get; set; }

    /// <summary>
    /// Gets or sets the proxy address for all protocols. Can be set via ALL_PROXY environment variable.
    /// </summary>
    public string? AllProxy { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of hostnames that should bypass the proxy. Can be set via NO_PROXY environment variable.
    /// </summary>
    public string? NoProxy { get; set; }

    /// <summary>
    /// Gets or sets the default timeout for HTTP requests. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets or sets the default User-Agent header value.
    /// </summary>
    public string? DefaultUserAgent { get; set; }
}
