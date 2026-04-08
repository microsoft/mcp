// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Microsoft.Mcp.Core.Services.Caching.Pagination;

/// <summary>
/// Central MCP resource that serves paginated results for any tool.
    /// Tools create a cursor via <see cref="IPaginationService"/> with a
    /// <see cref="PageFetchDelegate"/> on the <see cref="CursorRecord"/>,
/// and return the resource URI <c>pagination://pages/{cursor}</c> to the client.
/// The client then reads this resource to fetch pages.
/// </summary>
public sealed class PaginationResource(
    ICursorRegistry cursorRegistry,
    IPaginationService paginationService,
    ILogger<PaginationResource> logger) : McpServerResource
{
    /// <summary>
    /// Sentinel value stored as <see cref="CursorRecord.NativeState"/> for the
    /// initial cursor, indicating that the first page has not been fetched yet.
    /// </summary>
    public const string InitialNativeState = "__initial__";
    internal const string Scheme = "pagination";
    public const string UriPrefix = "pagination://pages/";
    private const string UriTemplateValue = "pagination://pages/{cursor}";
    private const string ResourceName = "pagination-pages";
    private const string ResourceDescription =
        "Fetches a page of results using an opaque cursor returned by a previous tool call or page read. " +
        "Pass the cursor value from the tool response or the previous page's nextPage URI.";

    private static readonly ResourceTemplate s_template = new()
    {
        Name = ResourceName,
        UriTemplate = UriTemplateValue,
        Description = ResourceDescription,
        MimeType = "application/json",
    };

    public override ResourceTemplate ProtocolResourceTemplate => s_template;

    public override IReadOnlyList<object> Metadata => [];

    public override bool IsMatch(string uri) =>
        uri.StartsWith(UriPrefix, StringComparison.OrdinalIgnoreCase);

    public override async ValueTask<ReadResourceResult> ReadAsync(
        RequestContext<ReadResourceRequestParams> request,
        CancellationToken cancellationToken = default)
    {
        var uri = request.Params!.Uri;
        var cursorId = ExtractCursorId(uri);

        if (string.IsNullOrEmpty(cursorId))
        {
            logger.LogWarning("Pagination resource read with missing cursor in URI: {Uri}", uri);
            return CreateErrorResult(uri, "Missing cursor parameter in URI.");
        }

        var record = await cursorRegistry.GetAsync(cursorId, cancellationToken);
        if (record is null)
        {
            logger.LogWarning("Cursor not found or expired: {CursorId}", cursorId);
            return CreateErrorResult(uri, "Cursor not found or expired.");
        }

        if (record.Fetcher is null)
        {
            logger.LogWarning("No page fetcher on cursor record: {CursorId}", cursorId);
            return CreateErrorResult(uri, "No page fetcher associated with this cursor.");
        }

        var fetcher = record.Fetcher;

        var nativeState = string.Equals(record.NativeState, InitialNativeState, StringComparison.Ordinal)
            ? null
            : record.NativeState;

        var pageData = await fetcher(nativeState, cancellationToken);

        string? nextPageUri = null;
        if (pageData.NextNativeState is not null)
        {
            var nextCursorId = await paginationService.SaveCursorAsync(
                record.Provider, record.Operation, record.RequestFingerprint,
                pageData.NextNativeState, fetcher, record.ResourceMetadata,
                cancellationToken: cancellationToken);

            nextPageUri = $"{UriPrefix}{nextCursorId}";
        }

        logger.LogDebug(
            "Pagination resource served page for cursor {CursorId} (operation={Operation}, hasNext={HasNext})",
            cursorId, record.Operation, nextPageUri is not null);

        var responseJson = nextPageUri is not null
            ? $$"""{"items":{{pageData.ItemsJson}},"nextPage":"{{nextPageUri}}"}"""
            : $$"""{"items":{{pageData.ItemsJson}}}""";

        return new ReadResourceResult
        {
            Contents =
            [
                new TextResourceContents
                {
                    Uri = uri,
                    MimeType = "application/json",
                    Text = responseJson,
                },
            ],
        };
    }

    internal static string? ExtractCursorId(string uri)
    {
        if (!uri.StartsWith(UriPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var cursorId = uri[UriPrefix.Length..];

        // Strip any query string if present
        var queryIndex = cursorId.IndexOf('?');
        if (queryIndex >= 0)
        {
            cursorId = cursorId[..queryIndex];
        }

        return string.IsNullOrEmpty(cursorId) ? null : cursorId;
    }

    private static ReadResourceResult CreateErrorResult(string uri, string message) =>
        new()
        {
            Contents =
            [
                new TextResourceContents
                {
                    Uri = uri,
                    MimeType = "application/json",
                    Text = $$"""{"error":"{{message}}"}""",
                },
            ],
        };
}
