// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Versioning;
using Azure.Mcp.Core.Areas.Server.Options;
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
    private string UserAgent { get; }

    public HttpClientService(IOptions<HttpClientOptions> options, IOptions<ServiceStartOptions> serviceStartOptions)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _defaultClient = new Lazy<HttpClient>(() => CreateClientInternal());

        var assembly = typeof(HttpClientService).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "unknown";
        var framework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "unknown";
        var platform = System.Runtime.InteropServices.RuntimeInformation.OSDescription;

        var transport = serviceStartOptions?.Value.Transport ?? TransportTypes.StdIo;
        UserAgent = $"azmcp/{version} azmcp-{transport}/{version} ({framework}; {platform})";
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

    /// <summary>
    /// Creates a new HttpClient instance with a customized function to acquire an access token for its outgoing requests.
    /// </summary>
    /// <param name="accessTokenProvider">A function to acquire access token.</param>
    /// <param name="baseAddress">The base address for the HttpClient</param>
    /// <returns>A new HttpClient instance.</returns>
    public HttpClient CreateClientWithAccessToken(Func<CancellationToken, Task<string>> accessTokenProvider, Uri? baseAddress)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(HttpClientService));
        }

        ArgumentNullException.ThrowIfNull(accessTokenProvider);

        var handler = CreateHttpClientHandler();

        var accessTokenHandler = new AccessTokenHandler(accessTokenProvider)
        {
            InnerHandler = handler
        };

        var client = new HttpClient(accessTokenHandler, disposeHandler: true);

        client.Timeout = _options.DefaultTimeout;
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

        if (baseAddress != null)
        {
            client.BaseAddress = baseAddress;
        }

        return client;
    }

    private HttpClient CreateClientInternal()
    {
        var handler = CreateHttpClientHandler();

#if DEBUG
        // If a TEST_PROXY_URL is configured, insert RecordingRedirectHandler as the last delegating handler
        var testProxyUrl = Environment.GetEnvironmentVariable("TEST_PROXY_URL");
        Console.WriteLine("Using test proxy URL: " + testProxyUrl);
        HttpMessageHandler pipeline = handler;
        if (!string.IsNullOrWhiteSpace(testProxyUrl) && Uri.TryCreate(testProxyUrl, UriKind.Absolute, out var proxyUri))
        {
            Console.WriteLine("Inserting RecordingRedirectHandler for test proxy.");
            // RecordingRedirectHandler should be the last delegating handler before the transport
            pipeline = new RecordingRedirectHandler(proxyUri)
            {
                InnerHandler = pipeline
            };
        }
        var client = new HttpClient(pipeline);
#else
        var client = new HttpClient(handler);
#endif

        // Apply default configuration
        client.Timeout = _options.DefaultTimeout;

        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

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

    /// <summary>
    /// DelegatingHandler that adds a Bearer access token to each outgoing request.
    /// </summary>
    private sealed class AccessTokenHandler : DelegatingHandler
    {
        private readonly Func<CancellationToken, Task<string>> _accessTokenProvider;

        public AccessTokenHandler(Func<CancellationToken, Task<string>> accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _accessTokenProvider(cancellationToken);
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
