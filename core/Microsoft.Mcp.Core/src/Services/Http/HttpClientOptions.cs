// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Configuration options for HttpClient services.
/// </summary>
/// <remarks>
/// The factory selects one explicit proxy in ALL, HTTPS, then HTTP precedence. It disables
/// transport-level SSRF protection for the whole handler, including requests excluded by
/// NoProxy. Without an explicit proxy, <see cref="HttpClient.DefaultProxy"/> is evaluated per
/// request and disables transport protection only when it selects a proxy. Endpoint validation
/// in callers remains independent. Use only trusted proxies and restrict their network access.
/// </remarks>
public sealed class HttpClientOptions
{
    /// <summary>
    /// Gets or sets the debug recording-proxy fallback captured from <c>TEST_PROXY_URL</c> at host configuration.
    /// </summary>
    /// <remarks>
    /// A fixture resolver result that is not <see langword="null"/> takes precedence.
    /// Release builds ignore this setting.
    /// Tests can configure it per provider without changing the process environment.
    /// Configuring a recording proxy disables transport-level SSRF protection, not endpoint validation.
    /// </remarks>
    public string? RecordingProxy { get; set; }

    /// <summary>
    /// Gets or sets the fallback explicit proxy address from the HTTP_PROXY environment variable.
    /// </summary>
    /// <remarks>
    /// The factory selects one handler-wide proxy in ALL_PROXY, HTTPS_PROXY, HTTP_PROXY order.
    /// Configuring an explicit proxy disables the shared handler's transport SSRF protection.
    /// </remarks>
    public string? HttpProxy { get; set; }

    /// <summary>
    /// Gets or sets the middle-precedence explicit proxy address from the HTTPS_PROXY environment variable.
    /// </summary>
    /// <remarks>
    /// The factory selects one handler-wide proxy in ALL_PROXY, HTTPS_PROXY, HTTP_PROXY order.
    /// Configuring an explicit proxy disables the shared handler's transport SSRF protection.
    /// </remarks>
    public string? HttpsProxy { get; set; }

    /// <summary>
    /// Gets or sets the highest-precedence explicit proxy address from the ALL_PROXY environment variable.
    /// </summary>
    /// <remarks>
    /// The factory selects one handler-wide proxy in ALL_PROXY, HTTPS_PROXY, HTTP_PROXY order.
    /// Configuring an explicit proxy disables the shared handler's transport SSRF protection.
    /// </remarks>
    public string? AllProxy { get; set; }

    /// <summary>
    /// Gets or sets the comma-separated list of hostnames that should bypass the proxy. Can be set via NO_PROXY environment variable.
    /// </summary>
    /// <remarks>Bypassing a configured proxy does not restore transport-level SSRF checks.</remarks>
    public string? NoProxy { get; set; }

    /// <summary>
    /// Gets or sets the default timeout for HTTP requests. Defaults to 100 seconds.
    /// </summary>
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Gets or sets the default User-Agent header value. 
    /// </summary>
    /// <remarks>
    /// UNUSED: This overwrites all user agents for all HTTP clients. So, it's not recommended to use this.
    /// </remarks>
    public string? DefaultUserAgent { get; set; }
}
