// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Validation;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.Core.Services;

public partial class FabricCoreService
{
    private const string InvalidSchedulePageMessage = "Fabric returned invalid schedule page metadata. No partial page was returned.";

    /// <inheritdoc />
    public async Task<ItemScheduleListCommandResult> ListItemSchedulesAsync(
        string workspaceId, string itemId, string jobType, string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var validation = new ValidationResult();
        ItemJobRunInput.ValidateContext(workspaceId, itemId, jobType, validation);
        if (!validation.IsValid)
        {
            throw new FabricItemInputException(string.Join(' ', validation.Errors));
        }
        if (!ItemJobInstanceListInput.IsValidContinuationToken(continuationToken))
        {
            throw new ArgumentException(ItemJobInstanceListInput.InvalidContinuationTokenMessage, nameof(continuationToken));
        }

        cancellationToken.ThrowIfCancellationRequested();
        var collection = new Uri($"{FabricEndpoints.GetFabricApiBaseUrl()}/workspaces/{Guid.Parse(workspaceId):D}/items/{Guid.Parse(itemId):D}/jobs/{Uri.EscapeDataString(jobType)}/schedules");
        var url = continuationToken is null
            ? collection.AbsoluteUri
            : $"{collection.AbsoluteUri}?continuationToken={FabricCoreHttpHelpers.EncodeContinuationToken(continuationToken)}";
        using var response = await SendFabricHttpRequestAsync(
            HttpMethod.Get, url, completionOption: HttpCompletionOption.ResponseHeadersRead, cancellationToken: cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var retryAfter = FabricCoreHttpHelpers.GetRetryAfter(response);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new ItemScheduleRequestException(
                response.IsSuccessStatusCode ? HttpStatusCode.BadGateway : response.StatusCode, retryAfter);
        }
        if (!string.Equals(response.Content.Headers.ContentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
        {
            throw new JsonException("Fabric returned a non-JSON schedule page.");
        }
        var body = await FabricResponseBodyReader.ReadAsync(
            response, FabricResponseBodyReader.DefaultMaxJsonBytes,
            static () => new JsonException("The Fabric schedule page exceeds the 1 MiB (1,048,576 byte) limit. No partial page was returned."),
            cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        var result = ReadItemSchedulesPage(body, collection, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return result with
        {
            RetryAfterSeconds = retryAfter?.Delta is { } delay ? (long)delay.TotalSeconds : null,
            RetryAfterUtc = retryAfter?.Date
        };
    }

    private static ItemScheduleListCommandResult ReadItemSchedulesPage(
        ReadOnlyMemory<byte> body, Uri collection, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions
            {
                MaxDepth = 64,
                AllowDuplicateProperties = false
            });
            var page = document.RootElement;
            FabricItemScheduleJson.ValidateUnicode(page);
            FabricItemScheduleJson.ValidateObject(page);
            if (!page.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException(InvalidSchedulePageMessage);
            }
            var token = ReadJobPageContinuationToken(page, collection, StringComparison.Ordinal);
            List<FabricItemSchedule> schedules = [];
            foreach (var entry in value.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                schedules.Add(FabricItemScheduleJson.Read(entry));
            }
            return new(schedules, token);
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            // JSON string/property decoding is lazy. Keep this boundary separate from
            // response reading and HTTP failures, whose status and retry guidance remain authoritative.
            throw new JsonException(InvalidSchedulePageMessage);
        }
    }

}
