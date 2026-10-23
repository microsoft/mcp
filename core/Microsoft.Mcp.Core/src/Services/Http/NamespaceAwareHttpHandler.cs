// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Security.AntiSSRF;

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Selects an isolated protected or unprotected transport for each outgoing request.
/// </summary>
/// <param name="protectedHandler">The terminal AntiSSRF transport with an immutable external-only policy.</param>
/// <param name="unprotectedHandler">A separate terminal transport for explicit namespace overrides.</param>
/// <param name="contextAccessor">The singleton accessor shared with command execution.</param>
/// <param name="ssrfProtectionPolicy">The immutable policy shared with this host's endpoint validator.</param>
/// <remarks>
/// The factory owns this handler and both connection pools. Never capture a command context
/// during construction: SDK clients and factory handlers can outlive the command that created them.
/// A missing or unresolved context always selects protection,
/// even when <see cref="SsrfProtectionPolicy.AllNamespaces"/> is configured.
/// Explicit and recording proxies are handled before this handler is constructed.
/// Destination-specific system and PAC proxy routing is handled by the outer
/// <see cref="ProxyRoutingHandler"/>, so this handler selects only by namespace.
/// </remarks>
/// <exception cref="ArgumentNullException">
/// An owned handler, <paramref name="contextAccessor"/>, or <paramref name="ssrfProtectionPolicy"/>
/// is <see langword="null"/>.
/// </exception>
internal sealed class NamespaceAwareHttpHandler(
    AntiSSRFHandler protectedHandler,
    SocketsHttpHandler unprotectedHandler,
    ICommandContextAccessor contextAccessor,
    SsrfProtectionPolicy ssrfProtectionPolicy) : HttpMessageHandler
{
    private readonly HttpMessageInvoker _protected = new(protectedHandler);
    private readonly HttpMessageInvoker _unprotected = new(unprotectedHandler);
    private readonly ICommandContextAccessor _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
    private readonly SsrfProtectionPolicy _ssrfProtectionPolicy = ssrfProtectionPolicy ?? throw new ArgumentNullException(nameof(ssrfProtectionPolicy));

    /// <summary>
    /// Disables redirects on both pools before the factory publishes the transport.
    /// </summary>
    /// <remarks>
    /// Configure once during handler creation, never after sending a request. Keeping both
    /// branches aligned prevents a namespace override from changing redirect behavior.
    /// </remarks>
    internal void DisableAutomaticRedirects()
    {
        protectedHandler.AllowAutoRedirect = false;
        unprotectedHandler.AllowAutoRedirect = false;
    }

    /// <summary>
    /// Sends synchronously using the namespace active at the start of this request.
    /// </summary>
    /// <param name="request">The request to pass unchanged to the selected transport.</param>
    /// <param name="cancellationToken">The token that cancels the send.</param>
    /// <returns>
    /// The selected transport's <see cref="HttpResponseMessage"/>.
    /// </returns>
    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        => SelectTransport().Send(request, cancellationToken);

    /// <summary>
    /// Sends asynchronously using the namespace active at the start of this request.
    /// </summary>
    /// <param name="request">The request to pass unchanged to the selected transport.</param>
    /// <param name="cancellationToken">The token that cancels the send.</param>
    /// <returns>
    /// The task producing the selected transport's <see cref="HttpResponseMessage"/>.
    /// </returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => SelectTransport().SendAsync(request, cancellationToken);

    /// <summary>
    /// Releases both owned transports when the factory retires this handler.
    /// </summary>
    /// <param name="disposing">Whether managed resources should be released.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _protected.Dispose();
            _unprotected.Dispose();
        }

        base.Dispose(disposing);
    }

    private HttpMessageInvoker SelectTransport()
        => _ssrfProtectionPolicy.AreSsrfProtectionsEnabled(_contextAccessor)
            ? _protected
            : _unprotected;
}
