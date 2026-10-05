// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text;

namespace Azure.Mcp.Tools.InfraIq.Tests.TestSupport;

/// <summary>
/// Fake HTTP transport that records every request and replies from a factory. No network is used.
/// </summary>
internal sealed class RecordingHttpMessageHandler(Func<HttpResponseMessage> responseFactory) : HttpMessageHandler
{
    private readonly List<RecordedHttpRequest> _requests = [];

    public IReadOnlyList<RecordedHttpRequest> Requests => _requests;

    public int CallCount => _requests.Count;

    public RecordedHttpRequest First => _requests[0];

    public static RecordingHttpMessageHandler Json(string body, HttpStatusCode status = HttpStatusCode.OK) =>
        new(() => new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        });

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
        _requests.Add(new RecordedHttpRequest(
            request.Method,
            request.RequestUri,
            request.Headers.Authorization?.Scheme,
            request.Headers.Authorization?.Parameter,
            request.Headers.ToDictionary(header => header.Key, header => string.Join(",", header.Value), StringComparer.OrdinalIgnoreCase),
            body));

        return responseFactory();
    }
}
