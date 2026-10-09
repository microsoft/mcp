// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Selects an isolated proxied or non-proxied transport for each outgoing request.
/// </summary>
/// <param name="nonProxiedHandler">
/// The namespace-aware non-proxy transport that retains transport DNS/IP validation unless
/// the current tool namespace has an emergency override.
/// </param>
/// <param name="proxiedHandler">
/// The transport configured with the runtime-provided system proxy and no transport DNS/IP validation.
/// </param>
/// <param name="systemProxy">
/// The runtime-provided proxy whose operating-system, PAC, and bypass rules are evaluated per destination.
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
/// <paramref name="nonProxiedHandler"/>, <paramref name="proxiedHandler"/>, or
/// <paramref name="systemProxy"/> is <see langword="null"/>.
/// </exception>
internal sealed class SystemProxyRoutingHandler(
    NamespaceAwareHttpHandler nonProxiedHandler,
    SocketsHttpHandler proxiedHandler,
    IWebProxy systemProxy) : HttpMessageHandler
{
    private readonly NamespaceAwareHttpHandler _nonProxiedHandler =
        nonProxiedHandler ?? throw new ArgumentNullException(nameof(nonProxiedHandler));
    private readonly SocketsHttpHandler _proxiedHandler =
        proxiedHandler ?? throw new ArgumentNullException(nameof(proxiedHandler));
    private readonly IWebProxy _systemProxy =
        systemProxy ?? throw new ArgumentNullException(nameof(systemProxy));
    private readonly HttpMessageInvoker _nonProxied = new(nonProxiedHandler);
    private readonly HttpMessageInvoker _proxied = new(proxiedHandler);

    /// <summary>
    /// Disables redirects on the non-proxied and proxied pools before publishing the ARM transport.
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
        // Evaluate the destination once because PAC resolution can be expensive and destination-specific.
        // IWebProxy permits null or the original destination to represent no proxy; requiring a
        // distinct proxy URI prevents a no-proxy PAC result from disabling transport IP validation.
        Uri? proxyUri = _systemProxy.GetProxy(requestUri);
        return proxyUri is not null && proxyUri != requestUri;
    }
}
