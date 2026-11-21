// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<ApiResourceGetCommandResult> GetApiResourceAsync(
        string? api,
        string path,
        int? maxResponseBytes = null,
        CancellationToken cancellationToken = default)
    {
        var target = ApiRequestInput.ParseApi(api);
        var uri = ApiRequestInput.BuildUri(target, path);
        var limit = ApiRequestInput.ParseMaxResponseBytes(maxResponseBytes);
        cancellationToken.ThrowIfCancellationRequested();

        using var response = await SendApiRequestAsync(target, HttpMethod.Get, uri, null, cancellationToken);
        return new(await FabricApiResponseReader.ReadAsync(target, response, limit, _timeProvider, cancellationToken));
    }

    private Task<HttpResponseMessage> SendApiRequestAsync(
        ApiRequestTarget target, HttpMethod method, Uri uri, string? jsonBody, CancellationToken cancellationToken) =>
        SendFabricHttpRequestAsync(
            method,
            uri.OriginalString,
            jsonBody,
            new() { ["Accept"] = "application/json" },
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken: cancellationToken,
            scopes: target.Scopes);
}
