// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    /// <inheritdoc />
    public async Task<FolderGetCommandResult> GetFolderAsync(
        string workspaceId, string folderId, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(folderId, out var folder) || folder == Guid.Empty)
        {
            throw new ArgumentException("Folder ID must be a nonempty UUID.", nameof(folderId));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var url = $"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/folders/{folder:D}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            if (response.IsSuccessStatusCode)
            {
                throw new FolderGetRequestException(HttpStatusCode.BadGateway, retryAfter);
            }
            try
            {
                var (code, unavailable) = await FabricCoreHttpHelpers.ReadErrorCodeAsync(response, cancellationToken);
                throw new FolderGetRequestException(response.StatusCode, retryAfter, code, unavailable);
            }
            catch (TimeoutException)
            {
                // Optional error metadata must not replace the status already received.
                throw new FolderGetRequestException(response.StatusCode, retryAfter, errorMetadataUnavailable: true);
            }
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Fabric returned non-JSON folder metadata.");
            }
            var bytes = await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("The Fabric folder metadata exceeds the 1 MiB response limit."), cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = ReadFolder(bytes, workspace, folder);
            cancellationToken.ThrowIfCancellationRequested();
            return new(snapshot, retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null, retryAfter?.Date);
        }
        catch (Exception ex) when (ex is JsonException or IOException or HttpRequestException or OperationCanceledException or TimeoutException)
        {
            var status = ex switch
            {
                JsonException => HttpStatusCode.BadGateway,
                OperationCanceledException => HttpStatusCode.RequestTimeout,
                TimeoutException => HttpStatusCode.GatewayTimeout,
                HttpRequestException { StatusCode: { } httpStatus } => httpStatus,
                _ => HttpStatusCode.ServiceUnavailable
            };
            throw new FolderGetRequestException(status, retryAfter);
        }
    }

    private static FabricFolder ReadFolder(ReadOnlyMemory<byte> bytes, Guid workspaceId, Guid folderId)
    {
        try
        {
            using var document = JsonDocument.Parse(bytes, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            FabricCoreHttpHelpers.ValidateMetadataJson(document.RootElement);
            var folder = document.RootElement.Deserialize(CoreJsonContext.Default.FabricFolder);
            // Read metadata has no creation-time name restrictions; preserve every valid string.
            if (folder is null || folder.Id != folderId || folder.WorkspaceId != workspaceId ||
                folder.DisplayName is null || folder.ParentFolderId == Guid.Empty)
            {
                throw new JsonException("Fabric returned invalid folder metadata.");
            }
            return folder;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // String/property decoding can fail lazily, including in omitted extension fields.
            throw new JsonException("Fabric returned invalid folder metadata.");
        }
    }
}
