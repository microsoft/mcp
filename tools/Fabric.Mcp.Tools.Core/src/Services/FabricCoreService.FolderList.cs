// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    private const string InvalidFolderPageMessage = "Fabric returned invalid folder page metadata. No partial page was returned.";

    /// <inheritdoc />
    public async Task<FolderListCommandResult> ListFoldersAsync(
        string workspaceId, string? rootFolderId = null, bool? recursive = null, string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        Guid? root = null;
        if (rootFolderId is not null)
        {
            if (!Guid.TryParse(rootFolderId, out var folder) || folder == Guid.Empty)
            {
                throw new ArgumentException("Root folder ID must be a nonempty UUID.", nameof(rootFolderId));
            }
            root = folder;
        }
        if (!FabricContinuationToken.IsValidContinuationToken(continuationToken))
        {
            throw new ArgumentException(FabricContinuationToken.InvalidContinuationTokenMessage, nameof(continuationToken));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var collection = new Uri($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/folders");
        List<string> query = [];
        if (root is { } rootId)
        {
            query.Add($"rootFolderId={rootId:D}");
        }
        if (recursive is { } includeNested)
        {
            query.Add($"recursive={(includeNested ? "true" : "false")}");
        }
        if (continuationToken is not null)
        {
            query.Add($"continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}");
        }
        var url = query.Count == 0 ? collection.AbsoluteUri : $"{collection.AbsoluteUri}?{string.Join('&', query)}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new FolderListRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }
        if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new JsonException("Fabric returned a non-JSON folder page.");
        }
        var body = await FabricResponseBodyReader.ReadAsync(
            response, FabricResponseBodyReader.DefaultMaxJsonBytes,
            static () => new JsonException("The Fabric folder page exceeds the 1 MiB (1,048,576 byte) limit. No partial page was returned."),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var result = ReadFoldersPage(body, collection, workspace, root, recursive ?? true, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return result with
        {
            RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
            RetryAfterUtc = retryAfter?.Date
        };
    }

    private static FolderListCommandResult ReadFoldersPage(
        ReadOnlyMemory<byte> body, Uri collection, Guid workspaceId, Guid? rootFolderId, bool recursive,
        CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            var page = document.RootElement;
            FabricCoreHttpHelpers.ValidateMetadataJson(page);
            if (page.ValueKind != JsonValueKind.Object ||
                !page.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException(InvalidFolderPageMessage);
            }
            var token = ReadFolderContinuationToken(page, collection, rootFolderId, recursive);
            List<FabricFolder> folders = [];
            foreach (var entry in value.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var folder = entry.Deserialize(CoreJsonContext.Default.FabricFolder);
                if (folder is null || folder.Id == Guid.Empty || folder.WorkspaceId != workspaceId ||
                    string.IsNullOrWhiteSpace(folder.DisplayName) || folder.ParentFolderId == Guid.Empty)
                {
                    throw new JsonException(InvalidFolderPageMessage);
                }
                folders.Add(folder);
            }
            return new(folders, token);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // JsonDocument decodes strings/property names lazily. Do not extend this catch
            // to response reading or HTTP errors, whose status and retry guidance are authoritative.
            throw new JsonException(InvalidFolderPageMessage);
        }
    }

    private static string? ReadFolderContinuationToken(JsonElement page, Uri collection, Guid? rootFolderId, bool recursive)
    {
        var hasToken = page.TryGetProperty("continuationToken", out var tokenValue) && tokenValue.ValueKind != JsonValueKind.Null;
        var hasUri = page.TryGetProperty("continuationUri", out var uriValue) && uriValue.ValueKind != JsonValueKind.Null;
        if (!hasToken)
        {
            if (hasUri)
            {
                throw new JsonException(InvalidFolderPageMessage);
            }
            return null;
        }
        if (tokenValue.ValueKind != JsonValueKind.String || tokenValue.GetString() is not { } token ||
            !FabricContinuationToken.IsValidContinuationToken(token) ||
            (hasUri && (uriValue.ValueKind != JsonValueKind.String ||
                !IsFolderContinuationUri(uriValue.GetString(), collection, token, rootFolderId, recursive))))
        {
            throw new JsonException(InvalidFolderPageMessage);
        }
        return token;
    }

    private static bool IsFolderContinuationUri(string? value, Uri collection, string token, Guid? rootFolderId, bool recursive)
    {
        var uri = FabricCoreHttpHelpers.GetMatchingCollectionUri(value, collection, StringComparison.OrdinalIgnoreCase);
        if (uri is null)
        {
            return false;
        }
        Dictionary<string, string> query = new(StringComparer.Ordinal);
        foreach (var pair in uri.Query[1..].Split('&'))
        {
            var separator = pair.IndexOf('=');
            if (separator <= 0 ||
                !query.TryAdd(pair[..separator], Uri.UnescapeDataString(pair[(separator + 1)..].Replace('+', ' '))))
            {
                return false;
            }
        }
        if (query.Keys.Any(static name => name is not ("continuationToken" or "rootFolderId" or "recursive")) ||
            !query.TryGetValue("continuationToken", out var actualToken) ||
            !string.Equals(actualToken, Uri.UnescapeDataString(FabricCoreHttpHelpers.EncodeContinuationToken(token)), StringComparison.Ordinal))
        {
            return false;
        }
        // Validate any filters present. Continuation always retains the caller's explicit
        // filters, even with cursor-only metadata; this URI never determines the next request.
        if (query.TryGetValue("rootFolderId", out var root) &&
            (!Guid.TryParse(root, out var folder) || folder != rootFolderId))
        {
            return false;
        }
        return !query.TryGetValue("recursive", out var nested) ||
            string.Equals(nested, recursive ? "true" : "false", StringComparison.OrdinalIgnoreCase);
    }
}
