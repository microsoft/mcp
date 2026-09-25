// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Azure.Mcp.Core.Services.Azure;

/// <summary>
/// Wraps the parsed JSON response of an Azure Resource Graph query.
/// Implements <see cref="IDisposable"/> to ensure the underlying <see cref="JsonDocument"/> is disposed.
/// </summary>
public sealed class ResourceGraphQueryResult : IDisposable
{
    private readonly JsonDocument _document;

    public ResourceGraphQueryResult(JsonDocument document)
    {
        _document = document ?? throw new ArgumentNullException(nameof(document));
        var root = document.RootElement;

        Data = root.TryGetProperty("data", out var data) ? data : default;

        if (root.TryGetProperty("count", out var countProp) && countProp.TryGetInt32(out var count))
        {
            Count = count;
        }
        else if (Data.ValueKind == JsonValueKind.Array)
        {
            Count = Data.GetArrayLength();
        }

        if (root.TryGetProperty("totalRecords", out var totalRecordsProp) && totalRecordsProp.TryGetInt64(out var totalRecords))
        {
            TotalRecords = totalRecords;
        }

        if (root.TryGetProperty("$skipToken", out var skipTokenProp) && skipTokenProp.ValueKind == JsonValueKind.String)
        {
            SkipToken = skipTokenProp.GetString();
        }

        if (root.TryGetProperty("facets", out var facetsProp))
        {
            Facets = facetsProp;
        }

        IsTruncated = root.TryGetProperty("resultTruncated", out var resultTruncated)
            && (resultTruncated.ValueKind == JsonValueKind.True
                || (resultTruncated.ValueKind == JsonValueKind.String
                    && string.Equals(resultTruncated.GetString(), "true", StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets the underlying <see cref="JsonDocument"/>.
    /// </summary>
    public JsonDocument Document => _document;

    /// <summary>
    /// Gets the data element containing the query results.
    /// </summary>
    public JsonElement Data { get; }

    /// <summary>
    /// Gets the number of records returned in this page.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the total number of records matching the query, if provided by the response.
    /// </summary>
    public long? TotalRecords { get; }

    /// <summary>
    /// Gets the continuation token for pagination, if any.
    /// </summary>
    public string? SkipToken { get; }

    /// <summary>
    /// Gets the facets element, if facets were requested.
    /// </summary>
    public JsonElement Facets { get; }

    /// <summary>
    /// Gets a value indicating whether the results were truncated.
    /// </summary>
    public bool IsTruncated { get; }

    /// <inheritdoc />
    public void Dispose() => _document.Dispose();
}
