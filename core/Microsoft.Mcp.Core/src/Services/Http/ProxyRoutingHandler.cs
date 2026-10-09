// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Selects an isolated proxied or fallback transport for each outgoing request.
/// </summary>
/// <param name="fallbackHandler">
/// The transport used when <paramref name="routeProxy"/> selects no proxy for the destination.
/// </param>
/// <param name="proxiedHandler">
/// The isolated transport configured with <paramref name="routeProxy"/>.
/// </param>
/// <param name="routeProxy">
/// The explicit or runtime proxy whose proxy and bypass rules are evaluated per destination.
/// </param>
/// <remarks>
/// Proxy applicability can be destination-specific because explicit bypass lists, operating-system
/// rules, and PAC scripts can select a proxy for one destination and no proxy for another.
/// The proxy is treated as the DNS/IP security boundary only for requests it actually routes.
/// Endpoint allow-list and public-target validation are separate from this transport decision
/// and remain active.
/// </remarks>
/// <exception cref="ArgumentNullException">
/// <paramref name="fallbackHandler"/>, <paramref name="proxiedHandler"/>, or
/// <paramref name="routeProxy"/> is <see langword="null"/>.
/// </exception>
internal sealed class ProxyRoutingHandler(
    HttpMessageHandler fallbackHandler,
    SocketsHttpHandler proxiedHandler,
    IWebProxy routeProxy) : HttpMessageHandler
{
    private readonly HttpMessageHandler _fallbackHandler =
        fallbackHandler ?? throw new ArgumentNullException(nameof(fallbackHandler));
    private readonly SocketsHttpHandler _proxiedHandler =
        proxiedHandler ?? throw new ArgumentNullException(nameof(proxiedHandler));
    private readonly IWebProxy _routeProxy =
        routeProxy ?? throw new ArgumentNullException(nameof(routeProxy));
    private readonly HttpMessageInvoker _fallback = new(fallbackHandler);
    private readonly HttpMessageInvoker _proxied = new(proxiedHandler);

    internal SocketsHttpHandler ProxiedHandler => _proxiedHandler;

    /// <summary>
    /// Disables redirects throughout the fallback and proxied routes before publishing the ARM transport.
    /// </summary>
    internal void DisableAutomaticRedirects()
    {
        DisableAutomaticRedirects(_fallbackHandler);
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
            _fallback.Dispose();
            _proxied.Dispose();
        }

        base.Dispose(disposing);
    }

    private static void DisableAutomaticRedirects(HttpMessageHandler handler)
    {
        switch (handler)
        {
            case NamespaceAwareHttpHandler namespaceHandler:
                namespaceHandler.DisableAutomaticRedirects();
                break;
            case ProxyRoutingHandler proxyRoutingHandler:
                proxyRoutingHandler.DisableAutomaticRedirects();
                break;
            case SocketsHttpHandler socketsHttpHandler:
                socketsHttpHandler.AllowAutoRedirect = false;
                break;
            default:
                throw new InvalidOperationException(
                    $"The proxy fallback handler type '{handler.GetType().FullName}' does not support disabling automatic redirects.");
        }
    }

    private HttpMessageInvoker SelectTransport(HttpRequestMessage request)
        => request.RequestUri is Uri requestUri && ShouldUseProxy(requestUri)
            ? _proxied
            : _fallback;

    private bool ShouldUseProxy(Uri requestUri)
    {
        // Evaluate the destination once because PAC resolution can be expensive and destination-specific.
        // IWebProxy permits null or the original destination to represent no proxy; requiring a
        // distinct proxy URI prevents a bypass result from entering the unfiltered proxied pool.
        Uri? proxyUri = _routeProxy.GetProxy(requestUri);
        return proxyUri is not null && proxyUri != requestUri;
    }
}
