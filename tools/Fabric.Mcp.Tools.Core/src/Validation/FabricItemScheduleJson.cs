// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Validation;

internal static class FabricItemScheduleJson
{
    private const string InvalidMetadataMessage = "Fabric returned invalid schedule metadata.";

    internal static FabricItemSchedule ReadMutationResponse(ReadOnlyMemory<byte> body)
    {
        try
        {
            using var document = JsonDocument.Parse(body, new JsonDocumentOptions { MaxDepth = 64, AllowDuplicateProperties = false });
            ValidateUnicode(document.RootElement);
            var schedule = Read(document.RootElement);
            // Reserve four levels for the result, namespace, tool-response, and JSON-RPC
            // envelopes. Call this inside the mutation's confirmed-success boundary.
            using var writer = new Utf8JsonWriter(Stream.Null, new JsonWriterOptions { MaxDepth = 60 });
            JsonSerializer.Serialize(writer, schedule, CoreJsonContext.Default.FabricItemSchedule);
            return schedule;
        }
        catch (Exception ex) when (ex is JsonException or InvalidOperationException)
        {
            throw new JsonException(InvalidMetadataMessage);
        }
    }

    internal static FabricItemSchedule Read(JsonElement value)
    {
        ValidateObject(value);
        var schedule = value.Deserialize(CoreJsonContext.Default.FabricItemSchedule);
        if (schedule is null || schedule.Id == Guid.Empty ||
            (schedule.CreatedDateTime is { } created && !TryReadTimestamp(created, out _)) ||
            (schedule.ExecutionData is { ValueKind: not JsonValueKind.Object }))
        {
            throw new JsonException(InvalidMetadataMessage);
        }
        if (schedule.Configuration is { } configuration)
        {
            ValidateConfiguration(configuration);
        }
        if (schedule.Owner is { } owner)
        {
            ValidateObject(owner);
            if (!owner.TryGetProperty("id", out var id) || id.ValueKind != JsonValueKind.String ||
                !id.TryGetGuid(out var ownerId) || ownerId == Guid.Empty)
            {
                throw new JsonException(InvalidMetadataMessage);
            }
            _ = ReadString(owner, "type");
        }
        return schedule;
    }

    internal static void ValidateConfiguration(JsonElement configuration)
    {
        ValidateObject(configuration);
        var type = ReadString(configuration, "type");
        _ = ReadString(configuration, "localTimeZoneId");
        if (!TryReadTimestamp(ReadString(configuration, "startDateTime"), out _) ||
            !TryReadTimestamp(ReadString(configuration, "endDateTime"), out _))
        {
            throw new JsonException(InvalidMetadataMessage);
        }

        // Validate documented shapes, not a closed type set or a local scheduling engine.
        switch (type)
        {
            case "Cron":
                ValidateInteger(configuration, "interval", 1, 5270400);
                break;
            case "Daily":
                ValidateStrings(configuration, "times", 100, times: true);
                break;
            case "Weekly":
                ValidateStrings(configuration, "times", 100, times: true);
                ValidateStrings(configuration, "weekdays", 7);
                break;
            case "Monthly":
                ValidateStrings(configuration, "times", 100, times: true);
                ValidateInteger(configuration, "recurrence", 1, 12);
                if (!configuration.TryGetProperty("occurrence", out var occurrence))
                {
                    throw new JsonException(InvalidMetadataMessage);
                }
                ValidateObject(occurrence);
                switch (ReadString(occurrence, "occurrenceType"))
                {
                    case "DayOfMonth":
                        ValidateInteger(occurrence, "dayOfMonth", 1, 31);
                        break;
                    case "OrdinalWeekday":
                        _ = ReadString(occurrence, "weekIndex");
                        _ = ReadString(occurrence, "weekday");
                        break;
                }
                break;
        }
    }

    private static void ValidateInteger(JsonElement value, string name, int min, int max)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Number ||
            !property.TryGetInt32(out var number) || number < min || number > max)
        {
            throw new JsonException(InvalidMetadataMessage);
        }
    }

    private static void ValidateStrings(JsonElement value, string name, int max, bool times = false)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.Array ||
            property.GetArrayLength() is 0 || property.GetArrayLength() > max)
        {
            throw new JsonException(InvalidMetadataMessage);
        }
        foreach (var entry in property.EnumerateArray())
        {
            if (entry.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(entry.GetString()) ||
                (times && !TimeOnly.TryParseExact(entry.GetString(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)))
            {
                throw new JsonException(InvalidMetadataMessage);
            }
        }
    }

    private static string ReadString(JsonElement value, string name)
    {
        if (!value.TryGetProperty(name, out var property) || property.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(property.GetString()))
        {
            throw new JsonException(InvalidMetadataMessage);
        }
        return property.GetString()!;
    }

    internal static bool TryReadTimestamp(string value, out DateTimeOffset timestamp) =>
        DateTimeOffset.TryParseExact(value,
            // cspell:disable-next-line
            ["yyyy-MM-dd'T'HH:mm:ss.FFFFFFF'Z'", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFzzz", "yyyy-MM-dd'T'HH:mm:ss.FFFFFFF"],
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out timestamp);

    internal static void ValidateObject(JsonElement value)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            throw new JsonException(InvalidMetadataMessage);
        }
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        foreach (var property in value.EnumerateObject())
        {
            if (!names.Add(property.Name))
            {
                throw new JsonException(InvalidMetadataMessage);
            }
        }
    }

    internal static void ValidateUnicode(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in value.EnumerateObject())
            {
                _ = property.Name;
                ValidateUnicode(property.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            foreach (var entry in value.EnumerateArray())
            {
                ValidateUnicode(entry);
            }
        }
        else if (value.ValueKind == JsonValueKind.String)
        {
            _ = value.GetString();
        }
    }
}
