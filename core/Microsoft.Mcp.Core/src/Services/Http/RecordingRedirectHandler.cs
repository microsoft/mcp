// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net.Http.Headers;
using Microsoft.Mcp.Core.Helpers;

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// DelegatingHandler that rewrites outgoing requests to a recording/replace proxy specified by TEST_PROXY_URL.
/// It also sets the x-recording-upstream-base-uri header once per request to preserve the original target.
///
/// This handler is intended to be injected as the LAST delegating handler (closest to the transport) so
/// that it rewrites the final outgoing wire request.
/// </summary>
/// <param name="proxyUri">The URI of the recording/replace proxy to redirect requests to.</param>
internal sealed class RecordingRedirectHandler(Uri proxyUri) : DelegatingHandler
{
    private const string CosmosSerializationHeader = "x-ms-cosmos-supported-serialization-formats";
    private readonly Uri _proxyUri = proxyUri ?? throw new ArgumentNullException(nameof(proxyUri));
    private readonly bool _playbackTesting = EnvironmentHelpers.IsPlaybackTesting();

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Redirect(request);
        return StripRetryAfter(base.Send(request, cancellationToken));
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Redirect(request);
        return StripRetryAfter(await base.SendAsync(request, cancellationToken).ConfigureAwait(false));
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

        if (message.Headers.Contains(CosmosSerializationHeader))
        {
            // Force Cosmos query responses to JSON so test proxy stores them accurately
            message.Headers.Remove(CosmosSerializationHeader);
        }

        // Rewrite target host/scheme/port
        var builder = new UriBuilder(_proxyUri)
        {
            Path = message.RequestUri!.AbsolutePath,
            Query = message.RequestUri!.Query?.TrimStart('?') ?? string.Empty
        };

        message.RequestUri = builder.Uri;
    }

    private HttpResponseMessage StripRetryAfter(HttpResponseMessage response)
    {
        if (_playbackTesting)
        {
            if (response.Headers.Remove("Retry-After"))
            {
                response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.Zero);
            }
            if (response.Headers.Remove("x-ms-retry-after-ms"))
            {
                response.Headers.Add("x-ms-retry-after-ms", "0");
            }
            if (response.Headers.Remove("retry-after-ms"))
            {
                response.Headers.Add("retry-after-ms", "0");
            }
        }
        return response;
    }
}
