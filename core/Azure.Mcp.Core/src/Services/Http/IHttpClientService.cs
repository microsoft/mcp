// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Services.Http;

/// <summary>
/// Service for providing configured HttpClient instances with centralized proxy and configuration management.
/// </summary>
public interface IHttpClientService
{
    /// <summary>
    /// Gets a default HttpClient instance configured with global settings.
    /// </summary>
    HttpClient DefaultClient { get; }

    /// <summary>
    /// Creates a new HttpClient instance with the specified base address and global configuration.
    /// </summary>
    /// <param name="baseAddress">The base address for the HttpClient.</param>
    /// <returns>A new HttpClient instance.</returns>
    HttpClient CreateClient(Uri? baseAddress = null);

    /// <summary>
    /// Creates a new HttpClient instance with the specified base address and additional configuration.
    /// </summary>
    /// <param name="baseAddress">The base address for the HttpClient.</param>
    /// <param name="configureClient">Additional configuration for the HttpClient.</param>
    /// <returns>A new HttpClient instance.</returns>
    HttpClient CreateClient(Uri? baseAddress, Action<HttpClient> configureClient);
}
