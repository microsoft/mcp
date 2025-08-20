// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Options;

namespace Azure.Mcp.Core.Services.Http;

/// <summary>
/// Implementation of IHttpClientService that provides configured HttpClient instances with proxy support.
/// </summary>
public sealed class HttpClientService : IHttpClientService, IDisposable
{
    private readonly HttpClientOptions _options;
    private readonly Lazy<HttpClient> _defaultClient;
    private bool _disposed;

    public HttpClientService(IOptions<HttpClientOptions> options)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _defaultClient = new Lazy<HttpClient>(() => CreateClientInternal());
    }

    /// <inheritdoc />
    public HttpClient DefaultClient => _defaultClient.Value;

    /// <inheritdoc />
    public HttpClient CreateClient(Uri? baseAddress = null)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HttpClientService));
        }

        var client = CreateClientInternal();
        if (baseAddress != null)
        {
            client.BaseAddress = baseAddress;
        }
        return client;
    }

    /// <inheritdoc />
    public HttpClient CreateClient(Uri? baseAddress, Action<HttpClient> configureClient)
    {
        ArgumentNullException.ThrowIfNull(configureClient);

        var client = CreateClient(baseAddress);
        configureClient(client);
        return client;
    }

    private HttpClient CreateClientInternal()
    {
        var handler = CreateHttpClientHandler();
        var client = new HttpClient(handler);

        // Apply default configuration
        client.Timeout = _options.DefaultTimeout;

        if (!string.IsNullOrEmpty(_options.DefaultUserAgent))
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(_options.DefaultUserAgent);
        }

        return client;
    }

    private HttpClientHandler CreateHttpClientHandler()
    {
        var handler = new HttpClientHandler();

        // Configure proxy settings
        var proxy = CreateProxy();
        if (proxy != null)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }

        return handler;
    }

    private WebProxy? CreateProxy()
    {
        // Determine proxy address based on priority: ALL_PROXY, HTTPS_PROXY, HTTP_PROXY
        string? proxyAddress = _options.AllProxy ?? _options.HttpsProxy ?? _options.HttpProxy;

        if (string.IsNullOrEmpty(proxyAddress))
        {
            return null;
        }

        if (!Uri.TryCreate(proxyAddress, UriKind.Absolute, out var proxyUri))
        {
            return null;
        }

        var proxy = new WebProxy(proxyUri);

        // Configure bypass list from NO_PROXY
        if (!string.IsNullOrEmpty(_options.NoProxy))
        {
            var bypassList = _options.NoProxy
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(ConvertGlobToRegex)
                .ToArray();

            if (bypassList.Length > 0)
            {
                proxy.BypassList = bypassList;
            }
        }

        return proxy;
    }

    /// <summary>
    /// Converts a glob-style pattern (e.g., "*.local") to a regex pattern for WebProxy.BypassList
    /// </summary>
    private static string ConvertGlobToRegex(string globPattern)
    {
        if (string.IsNullOrEmpty(globPattern))
        {
            return string.Empty;
        }

        // Escape regex special characters except * and ?
        var escaped = globPattern
            .Replace("\\", "\\\\")
            .Replace(".", "\\.")
            .Replace("+", "\\+")
            .Replace("$", "\\$")
            .Replace("^", "\\^")
            .Replace("{", "\\{")
            .Replace("}", "\\}")
            .Replace("[", "\\[")
            .Replace("]", "\\]")
            .Replace("(", "\\(")
            .Replace(")", "\\)")
            .Replace("|", "\\|");

        // Convert glob wildcards to regex
        // * means "match any number of characters"
        // ? means "match any single character"
        var regex = escaped
            .Replace("*", ".*")
            .Replace("?", ".");

        // Anchor the pattern to match the entire string
        return $"^{regex}$";
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (_defaultClient.IsValueCreated)
            {
                _defaultClient.Value.Dispose();
            }
            _disposed = true;
        }
    }
}
