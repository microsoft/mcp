// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
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
            ValidateScheduleJsonUnicode(page);
            ValidateScheduleMetadataObject(page);
            if (!page.TryGetProperty("value", out var value) || value.ValueKind != JsonValueKind.Array)
            {
                throw new JsonException(InvalidSchedulePageMessage);
            }
            var token = ReadJobPageContinuationToken(page, collection, StringComparison.Ordinal);
            List<FabricItemSchedule> schedules = [];
            foreach (var entry in value.EnumerateArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                ValidateScheduleMetadataObject(entry);
                var schedule = entry.Deserialize(CoreJsonContext.Default.FabricItemSchedule);
                if (schedule is null || schedule.Id == Guid.Empty ||
                    (schedule.CreatedDateTime is { } created && !IsScheduleTimestamp(created)) ||
                    (schedule.ExecutionData is { ValueKind: not JsonValueKind.Object }))
                {
                    throw new JsonException(InvalidSchedulePageMessage);
                }
                if (schedule.Configuration is { } configuration)
                {
                    ValidateScheduleConfiguration(configuration);
                }
                if (schedule.Owner is { } owner)
                {
                    ValidateScheduleMetadataObject(owner);
                    if (!owner.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String ||
                        !id.TryGetGuid(out var ownerId) || ownerId == Guid.Empty)
                    {
                        throw new JsonException(InvalidSchedulePageMessage);
                    }
                    _ = ReadScheduleString(owner, "type");
                }
                schedules.Add(schedule);
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

    private static void ValidateScheduleConfiguration(JsonElement configuration)
    {
        ValidateScheduleMetadataObject(configuration);
        var type = ReadScheduleString(configuration, "type");
        _ = ReadScheduleString(configuration, "localTimeZoneId");
        if (!IsScheduleTimestamp(ReadScheduleString(configuration, "startDateTime")) ||
            !IsScheduleTimestamp(ReadScheduleString(configuration, "endDateTime")))
        {
            throw new JsonException(InvalidSchedulePageMessage);
        }

        // Validate documented shapes, not a closed type set or a local scheduling engine.
        switch (type)
        {
            case "Cron":
                ValidateScheduleInteger(configuration, "interval", 1, 5270400);
                break;
            case "Daily":
                ValidateScheduleStrings(configuration, "times", 100, times: true);
                break;
            case "Weekly":
                ValidateScheduleStrings(configuration, "times", 100, times: true);
                ValidateScheduleStrings(configuration, "weekdays", 7);
                break;
            case "Monthly":
                ValidateScheduleStrings(configuration, "times", 100, times: true);
                ValidateScheduleInteger(configuration, "recurrence", 1, 12);
                if (!configuration.TryGetProperty("occurrence", out var occurrence))
                {
                    throw new JsonException(InvalidSchedulePageMessage);
                }
                ValidateScheduleMetadataObject(occurrence);
                switch (ReadScheduleString(occurrence, "occurrenceType"))
                {
                    case "DayOfMonth":
                        ValidateScheduleInteger(occurrence, "dayOfMonth", 1, 31);
                        break;
                    case "OrdinalWeekday":
                        _ = ReadScheduleString(occurrence, "weekIndex");
                        _ = ReadScheduleString(occurrence, "weekday");
                        break;
                }
                break;
        }
    }

    private static void ValidateScheduleInteger(JsonElement value, string name, int min, int max)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var number) || number < min || number > max)
        {
            throw new JsonException(InvalidSchedulePageMessage);
        }
    }

    private static void ValidateScheduleStrings(JsonElement value, string name, int max, bool times = false)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Array ||
            property.GetArrayLength() is 0 || property.GetArrayLength() > max)
        {
            throw new JsonException(InvalidSchedulePageMessage);
        }
        foreach (var entry in property.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(entry.GetString()) ||
                (times && !TimeOnly.TryParseExact(entry.GetString(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)))
            {
                throw new JsonException(InvalidSchedulePageMessage);
            }
        }
    }

    private static string ReadScheduleString(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new JsonException(InvalidSchedulePageMessage);
        }
        return property.GetString()!;
    }

    private static bool IsScheduleTimestamp(string value) =>
        DateTimeOffset.TryParseExact(value,
            // cspell:disable-next-line
            ["yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"],
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out _);

    private static void ValidateScheduleMetadataObject(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(InvalidSchedulePageMessage);
        }
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        foreach (var property in value.EnumerateObject())
        {
            if (!names.Add(property.Name))
            {
                throw new JsonException(InvalidSchedulePageMessage);
            }
        }
    }

    private static void ValidateScheduleJsonUnicode(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in value.EnumerateObject())
            {
                _ = property.Name;
                ValidateScheduleJsonUnicode(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in value.EnumerateArray())
            {
                ValidateScheduleJsonUnicode(entry);
            }
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            _ = value.GetString();
        }
    }
}
