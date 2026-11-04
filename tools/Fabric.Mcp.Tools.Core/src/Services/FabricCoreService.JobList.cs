// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    private const string InvalidJobPageMessage = "Fabric returned invalid job-instance page metadata. No partial page was returned.";

    /// <inheritdoc />
    public async Task<ItemJobInstanceListCommandResult> ListItemJobInstancesAsync(
        string workspaceId, string itemId, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(workspaceId, out var workspace) || workspace == Guid.Empty)
        {
            throw new ArgumentException("Workspace ID must be a nonempty UUID.", nameof(workspaceId));
        }
        if (!Guid.TryParse(itemId, out var item) || item == Guid.Empty)
        {
            throw new ArgumentException("Item ID must be a nonempty UUID.", nameof(itemId));
        }
        if (!ItemJobInstanceListInput.IsValidContinuationToken(continuationToken))
        {
            throw new ArgumentException(ItemJobInstanceListInput.InvalidContinuationTokenMessage, nameof(continuationToken));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var collection = new Uri($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{workspace:D}/items/{item:D}/jobs/instances");
        var url = continuationToken is null
            ? collection.AbsoluteUri
            : $"{collection.AbsoluteUri}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ItemJobInstanceRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }
        if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new JsonException("Fabric returned a non-JSON job-instance page.");
        }
        var body = await FabricResponseBodyReader.ReadAsync(
            response, FabricResponseBodyReader.DefaultMaxJsonBytes,
            static () => new JsonException("The Fabric job-instance page exceeds the 1 MiB (1,048,576 byte) limit. No partial page was returned."),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var result = ReadItemJobInstancesPage(body, collection, item, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return result with
        {
            RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
            RetryAfterUtc = retryAfter?.Date
        };
    }

    private static ItemJobInstanceListCommandResult ReadItemJobInstancesPage(
        ReadOnlyMemory<byte> body, Uri collection, Guid itemId, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            var page = document.RootElement;
            ValidateJobPageJson(page);
            if (page.ValueKind != JsonValueKind.Object ||
                !page.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException(InvalidJobPageMessage);
            }
            var token = ReadJobPageContinuationToken(page, collection);
            List<FabricItemJobInstance> instances = [];
            foreach (var entry in value.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                instances.Add(ValidateItemJobInstance(entry.Deserialize(CoreJsonContext.Default.FabricItemJobInstance), itemId));
            }
            return new(instances, token);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // JsonDocument decodes names and UTF-16 escapes lazily. This catch is confined to
            // required JSON inspection, never response reading or optional HTTP error metadata.
            throw new JsonException(InvalidJobPageMessage);
        }
    }

    private static void ValidateJobPageJson(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
            foreach (var property in value.EnumerateObject())
            {
                if (!names.Add(property.Name))
                {
                    throw new JsonException(InvalidJobPageMessage);
                }
                ValidateJobPageJson(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in value.EnumerateArray())
            {
                ValidateJobPageJson(entry);
            }
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            // Validate even strings in fields omitted from the safe output projection.
            _ = value.GetString();
        }
    }

    private static string? ReadJobPageContinuationToken(JsonElement page, Uri collection)
    {
        var hasToken = page.TryGetProperty("continuationToken", out var tokenValue) && tokenValue.ValueKind != JsonValueKind.Null;
        var hasUri = page.TryGetProperty("continuationUri", out var uriValue) && uriValue.ValueKind != JsonValueKind.Null;
        if (!hasToken)
        {
            if (hasUri)
            {
                throw new JsonException(InvalidJobPageMessage);
            }
            return null;
        }
        if (tokenValue.ValueKind != JsonValueKind.String || tokenValue.GetString() is not { } token ||
            !ItemJobInstanceListInput.IsValidContinuationToken(token) ||
            (hasUri && (uriValue.ValueKind != JsonValueKind.String ||
                !IsJobPageContinuationUri(uriValue.GetString(), collection, token))))
        {
            throw new JsonException(InvalidJobPageMessage);
        }
        return token;
    }

    private static bool IsJobPageContinuationUri(string? value, Uri collection, string token)
    {
        if (string.IsNullOrEmpty(value) || value.Any(static ch => char.IsWhiteSpace(ch) || char.IsControl(ch)) ||
            value.Contains('\\'))
        {
            return false;
        }
        var queryStart = value.IndexOf('?');
        if (queryStart < 0)
        {
            return false;
        }
        var address = value.AsSpan(0, queryStart);
        if (address.Contains('%') || address.Contains("/./", StringComparison.Ordinal) || address.Contains("/../", StringComparison.Ordinal) ||
            !Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != collection.Scheme ||
            !string.Equals(uri.Host, collection.Host, StringComparison.OrdinalIgnoreCase) || uri.Port != collection.Port ||
            uri.UserInfo.Length != 0 || uri.Fragment.Length != 0 ||
            !string.Equals(uri.AbsolutePath, collection.AbsolutePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        const string prefix = "?continuationToken=";
        if (!uri.Query.StartsWith(prefix, StringComparison.Ordinal) || uri.Query.Contains('&'))
        {
            return false;
        }

        // Compare one query value after one decoding step, including Fabric's already-escaped
        // tokens. The URL is neither returned nor followed; subsequent requests use only the token.
        var actual = Uri.UnescapeDataString(uri.Query[prefix.Length..].Replace('+', ' '));
        var expected = Uri.UnescapeDataString(FabricCoreHttpHelpers.EncodeContinuationToken(token));
        return string.Equals(actual, expected, StringComparison.Ordinal);
    }
}
