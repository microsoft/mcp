// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Selects an isolated proxied or non-proxied transport for each outgoing request.
/// </summary>
/// <param name="proxiedHandler">
/// The transport configured with an explicit or runtime-provided proxy and no transport DNS/IP validation.
/// </param>
/// <param name="nonProxiedHandler">
/// The namespace-aware non-proxy transport that retains transport DNS/IP validation unless
/// the current tool namespace has an emergency override.
/// </param>
/// <remarks>
/// Proxy applicability cannot be determined when the DI container or handler is constructed because
/// PAC scripts and bypass rules can select a proxy for one destination and no proxy for another.
/// The proxy is treated as the DNS/IP security boundary only for requests it actually routes.
/// Requests not routed through a proxy continue through <paramref name="nonProxiedHandler"/>.
/// Endpoint allow-list and public-target validation are separate from this transport decision
/// and remain active.
/// </remarks>
/// <exception cref="ArgumentNullException">
/// <paramref name="nonProxiedHandler"/> or <paramref name="proxiedHandler"/> is <see langword="null"/>.
/// </exception>
/// <exception cref="ArgumentException">
/// <paramref name="proxiedHandler"/> does not have proxy use enabled with an explicit
/// <see cref="IWebProxy"/> instance.
/// </exception>
internal sealed class ProxyRoutingHandler(
    SocketsHttpHandler proxiedHandler,
    NamespaceAwareHttpHandler nonProxiedHandler) : HttpMessageHandler
{
    private readonly SocketsHttpHandler _proxiedHandler =
        proxiedHandler ?? throw new ArgumentNullException(nameof(proxiedHandler));
    private readonly NamespaceAwareHttpHandler _nonProxiedHandler =
        nonProxiedHandler ?? throw new ArgumentNullException(nameof(nonProxiedHandler));
    private readonly IWebProxy _proxy = GetProxy(proxiedHandler);
    private readonly HttpMessageInvoker _nonProxied = new(nonProxiedHandler);
    private readonly HttpMessageInvoker _proxied = new(proxiedHandler);

    /// <summary>
    /// Gets the terminal transport used for requests routed through the proxy.
    /// </summary>
    /// <remarks>
    /// Exposed internally so unit tests can inspect transport configuration and verify
    /// that different factory client names receive independent connection pools.
    /// </remarks>
    internal SocketsHttpHandler ProxiedHandler => _proxiedHandler;

    /// <summary>
    /// Disables redirects on the non-proxied and proxied pools before publishing the transport.
    /// </summary>
    internal void DisableAutomaticRedirects()
    {
        _nonProxiedHandler.DisableAutomaticRedirects();
        _proxiedHandler.AllowAutoRedirect = false;
    }

    /// <inheritdoc/>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        => SelectTransport(request).Send(request, cancellationToken);

    /// <inheritdoc/>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
        => SelectTransport(request).SendAsync(request, cancellationToken);

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _nonProxied.Dispose();
            _proxied.Dispose();
        }

        base.Dispose(disposing);
    }

    private HttpMessageInvoker SelectTransport(HttpRequestMessage request)
        => request.RequestUri is Uri requestUri && ShouldUseProxy(requestUri)
            ? _proxied
            : _nonProxied;

    private bool ShouldUseProxy(Uri requestUri)
    {
        // Evaluate the same proxy object used by the transport so routing cannot disagree
        // because independently supplied proxy instances have different PAC or bypass state.
        // Null or the original destination represents a direct route, which must retain filtering.
        Uri? proxyUri = _proxy.GetProxy(requestUri);
        return proxyUri is not null && proxyUri != requestUri;
    }

    private static IWebProxy GetProxy(SocketsHttpHandler proxiedHandler)
    {
        ArgumentNullException.ThrowIfNull(proxiedHandler);

        if (!proxiedHandler.UseProxy || proxiedHandler.Proxy is not IWebProxy proxy)
        {
            throw new ArgumentException(
                "The proxied transport must enable proxy use with an explicit proxy instance.",
                nameof(proxiedHandler));
        }

        return proxy;
    }
}
