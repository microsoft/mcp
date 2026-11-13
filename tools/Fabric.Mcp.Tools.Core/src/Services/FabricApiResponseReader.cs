// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>
/// Reads bounded JSON responses from allowlisted REST APIs and reduces failures to sanitized metadata.
/// </summary>
internal static class FabricApiResponseReader
{
    internal const int MaxJsonDepth = 64;

    /// <summary>
    /// The deepest JSON value that can be returned. Retained values sit inside five output envelopes
    /// (JSON-RPC message, tool result, structured content, namespace result, and response or operation
    /// result), so they are limited before the command succeeds instead of failing during serialization.
    /// </summary>
    internal const int MaxRetainedJsonDepth = MaxJsonDepth - 5;
    private static readonly byte[] s_utf8Bom = [0xEF, 0xBB, 0xBF];

    internal static async Task<FabricApiResponse> ReadAsync(
        ApiRequestTarget target, HttpResponseMessage response, int maxBytes, TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var status = (int)response.StatusCode;
        var requestId = GetRequestId(response);
        if (status is >= 300 and < 400)
        {
            // Redirects are never followed, so their targets are not returned either.
            throw new ApiRequestException(ApiRequestFailure.Redirect, HttpStatusCode.BadGateway, status, requestId: requestId);
        }
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateServiceErrorAsync(response, requestId, cancellationToken);
        }

        string? location = null;
        double? retryAfterSeconds = null;
        if (response.StatusCode == HttpStatusCode.Accepted)
        {
            location = GetLocation(target, response);
            retryAfterSeconds = FabricOperationHttp.GetRetryAfterSeconds(response, timeProvider);
        }

        var body = await ReadJsonBodyAsync(response, status, requestId, maxBytes, cancellationToken);
        if (body.IsEmpty)
        {
            return new(target.Name, status, false, default, null, location, retryAfterSeconds, requestId);
        }

        JsonElement value;
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = MaxRetainedJsonDepth });
            ValidateStrings(document.RootElement);
            value = document.RootElement.Clone();
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new ApiRequestException(ApiRequestFailure.InvalidJson, HttpStatusCode.BadGateway, status, requestId: requestId);
        }
        return new(target.Name, status, true, value, GetNextPath(target, value), location, retryAfterSeconds, requestId);
    }

    /// <summary>Rejects an already parsed value that is too deep to return inside the output envelopes.</summary>
    internal static JsonElement EnsureRetainedDepth(JsonElement value)
    {
        try
        {
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = MaxRetainedJsonDepth });
            value.WriteTo(writer);
        }
        catch (InvalidOperationException)
        {
            throw new FabricOperationException("InvalidJson", FormattableString.Invariant(
                $"Fabric returned an operation result deeper than {MaxRetainedJsonDepth} JSON nesting levels. The result was not returned; do not resubmit the original request."));
        }
        return value;
    }

    internal static string? GetRequestId(HttpResponseMessage response)
    {
        foreach (var name in (string[])["x-ms-request-id", "RequestId"])
        {
            if (response.Headers.NonValidated.TryGetValues(name, out var values) && values.Count == 1 &&
                values.Single() is var id && FabricCoreHttpHelpers.IsSafeErrorCode(id))
            {
                return id;
            }
        }
        return null;
    }

    internal static string? GetNextPath(ApiRequestTarget target, JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            return null;
        }
        foreach (var name in (string[])["continuationUri", "@odata.nextLink"])
        {
            if (value.TryGetProperty(name, out var link) && link.ValueKind == JsonValueKind.String &&
                ApiRequestInput.GetRelativeLink(target, link.GetString()) is { } path)
            {
                return path;
            }
        }
        return null;
    }

    private static string? GetLocation(ApiRequestTarget target, HttpResponseMessage response) =>
        response.Headers.NonValidated.TryGetValues("Location", out var values) && values.Count == 1
            ? ApiRequestInput.GetRelativeLink(target, values.Single())
            : null;

    private static bool IsJsonMediaType(string? mediaType) =>
        mediaType is not null &&
        (mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.Equals("text/json", StringComparison.OrdinalIgnoreCase) ||
            mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase));

    private static async Task<ReadOnlyMemory<byte>> ReadJsonBodyAsync(
        HttpResponseMessage response, int status, string? requestId, int maxBytes, CancellationToken cancellationToken)
    {
        if (response.Content.Headers.ContentLength == 0)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        ApiRequestException Unsupported() =>
            new(ApiRequestFailure.UnsupportedMediaType, HttpStatusCode.BadGateway, status, requestId: requestId);

        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is null)
        {
            // Without a media type, only an empty body is accepted; read at most one byte to find out.
            var probe = await FabricResponseBodyReader.ReadAsync(response, 1, Unsupported, cancellationToken);
            return probe.IsEmpty ? probe : throw Unsupported();
        }
        if (!IsJsonMediaType(mediaType))
        {
            throw Unsupported();
        }

        var body = await FabricResponseBodyReader.ReadAsync(
            response, maxBytes,
            () => new ApiRequestException(ApiRequestFailure.ResponseTooLarge, HttpStatusCode.BadGateway, status, requestId: requestId),
            cancellationToken);
        body = SkipBom(body);
        return body.Span.IndexOfAnyExcept(" \t\r\n"u8) < 0 ? ReadOnlyMemory<byte>.Empty : body;
    }

    private static ReadOnlyMemory<byte> SkipBom(ReadOnlyMemory<byte> body) =>
        body.Span.StartsWith(s_utf8Bom) ? body[s_utf8Bom.Length..] : body;

    private static async Task<ApiRequestException> CreateServiceErrorAsync(
        HttpResponseMessage response, string? requestId, CancellationToken cancellationToken)
    {
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        var (errorCode, bodyRequestId) = await ReadErrorMetadataAsync(response, cancellationToken);
        return new(
            ApiRequestFailure.ServiceError, response.StatusCode, (int)response.StatusCode, retryAfter,
            errorCode, requestId ?? bodyRequestId, errorMetadataUnavailable: errorCode is null);
    }

    /// <summary>Reads only a validated error code and request ID; backend messages are never read into results.</summary>
    private static async Task<(string? ErrorCode, string? RequestId)> ReadErrorMetadataAsync(
        HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            if (!IsJsonMediaType(response.Content.Headers.ContentType?.MediaType))
            {
                return (null, null);
            }
            var body = SkipBom(await FabricResponseBodyReader.ReadAsync(
                response, FabricResponseBodyReader.DefaultMaxJsonBytes,
                static () => new JsonException("REST error metadata exceeds the 1 MiB limit."),
                cancellationToken));
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = MaxJsonDepth });
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return (null, null);
            }

            // Fabric uses a root errorCode; Power BI uses error.code.
            var code = GetSafeString(root, "errorCode");
            if (code is null && root.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
            {
                code = GetSafeString(error, "code");
            }
            return (code, GetSafeString(root, "requestId"));
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException or IOException or HttpRequestException or OperationCanceledException)
        {
            // Optional metadata must not replace a known HTTP status or Retry-After.
            return (null, null);
        }
    }

    private static string? GetSafeString(JsonElement element, string name) =>
        element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String &&
        value.GetString() is { } text && FabricCoreHttpHelpers.IsSafeErrorCode(text)
            ? text
            : null;

    private static void ValidateStrings(JsonElement value)
    {
        // JSON string decoding is lazy; force it so invalid UTF-8 fails here instead of during output.
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in value.EnumerateObject())
                {
                    _ = property.Name;
                    ValidateStrings(property.Value);
                }
                break;
            case JsonValueKind.Array:
                foreach (var entry in value.EnumerateArray())
                {
                    ValidateStrings(entry);
                }
                break;
            case JsonValueKind.String:
                _ = value.GetString();
                break;
        }
    }
}
