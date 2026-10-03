// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Pricing.Tests.TestSupport;

internal sealed class RecordingHttpMessageHandler(
    Func<HttpRequestMessage, int, HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    public List<Uri> RequestUris { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        RequestUris.Add(request.RequestUri!);
        return Task.FromResult(responseFactory(request, RequestUris.Count));
    }
}
