// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;

namespace Azure.Mcp.Core.Services.Http;

/// <summary>
/// DelegatingHandler that rewrites outgoing requests to a recording/replace proxy specified by TEST_PROXY_URL.
/// It also sets the x-recording-upstream-base-uri header once per request to preserve the original target.
/// 
/// This handler is intended to be injected as the LAST delegating handler (closest to the transport) so
/// that it rewrites the final outgoing wire request.
/// </summary>
internal sealed class RecordingRedirectHandler : DelegatingHandler
{
    private readonly Uri _proxyUri;

    public RecordingRedirectHandler(Uri proxyUri)
    {
        _proxyUri = proxyUri ?? throw new ArgumentNullException(nameof(proxyUri));
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Redirect(request);
        return base.Send(request, cancellationToken)!;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Redirect(request);
        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private void Redirect(HttpRequestMessage message)
    {
        // Only set upstream header once (HttpRequestMessage can be cloned/reused by some handlers)
        if (!message.Headers.Contains("x-recording-upstream-base-uri"))
        {
            var upstream = new UriBuilder(message.RequestUri!)
            {
                Query = string.Empty,
                Path = string.Empty
            };
            message.Headers.Add("x-recording-upstream-base-uri", upstream.Uri.ToString());
        }

        // Rewrite target host/scheme/port
        var builder = new UriBuilder(_proxyUri)
        {
            Path = message.RequestUri!.AbsolutePath,
            Query = message.RequestUri!.Query?.TrimStart('?') ?? string.Empty
        };

        message.RequestUri = builder.Uri;
    }
}
