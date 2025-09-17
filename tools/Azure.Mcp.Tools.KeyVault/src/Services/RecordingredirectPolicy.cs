// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;

namespace Azure.Mcp.Tools.KeyVault.Services;

/// <summary>
/// Minimal redirect policy that only sets x-recording-upstream-base-uri once and rewrites the request to the proxy
/// configured by TEST_PROXY_URL. No other recording headers or behaviors are added here.
/// </summary>
internal sealed class RecordingRedirectPolicy : HttpPipelinePolicy
{
    private readonly Uri _proxyUri;

    public RecordingRedirectPolicy(Uri proxyUri)
    {
        _proxyUri = proxyUri;
    }

    public override void Process(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        Redirect(message);
        ProcessNext(message, pipeline);
    }

    public override ValueTask ProcessAsync(HttpMessage message, ReadOnlyMemory<HttpPipelinePolicy> pipeline)
    {
        Redirect(message);
        return ProcessNextAsync(message, pipeline);
    }

    private void Redirect(HttpMessage message)
    {
        // Only set upstream header once (message is reused for retries)
        if (!message.Request.Headers.Contains("x-recording-upstream-base-uri"))
        {
            var upstream = new RequestUriBuilder
            {
                Scheme = message.Request.Uri.Scheme,
                Host = message.Request.Uri.Host,
                Port = message.Request.Uri.Port
            };
            message.Request.Headers.SetValue("x-recording-upstream-base-uri", upstream.ToString());
        }

        message.Request.Uri.Scheme = _proxyUri.Scheme;
        message.Request.Uri.Host = _proxyUri.Host;
        message.Request.Uri.Port = _proxyUri.IsDefaultPort ? _proxyUri.Port : _proxyUri.Port;
    }
}
