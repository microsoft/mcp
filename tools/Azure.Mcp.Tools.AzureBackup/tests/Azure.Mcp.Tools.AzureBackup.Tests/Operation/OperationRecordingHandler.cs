// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Tests.Operation;

// The SDK setup call runs in the test process, not the MCP server. Route it through
// the SAME recording session so playback never needs a live credential or VM write.
internal sealed class OperationRecordingHandler(Uri proxy, string recordingId, string mode) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var upstream = request.RequestUri!;
        request.Headers.TryAddWithoutValidation("x-recording-upstream-base-uri", upstream.GetLeftPart(UriPartial.Authority));
        request.Headers.TryAddWithoutValidation("x-recording-id", recordingId);
        request.Headers.TryAddWithoutValidation("x-recording-mode", mode);
        request.RequestUri = new UriBuilder(proxy) { Path = upstream.AbsolutePath, Query = upstream.Query.TrimStart('?') }.Uri;
        return await base.SendAsync(request, cancellationToken);
    }
}
