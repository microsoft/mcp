// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.ResourceHealth.Models.Internal;

/// <summary>
/// A JSON converter that handles DateTimeOffset values which may lack timezone information.
/// The Azure Resource Health API can return datetime strings without timezone designators
/// (e.g., "0001-01-01T00:00:00" for unmitigated events), which System.Text.Json cannot
/// parse to DateTimeOffset by default. This converter assumes UTC for such values.
/// </summary>
internal sealed class LenientNullableDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Unexpected token parsing {nameof(DateTimeOffset)}?. Expected String or Null, got {reader.TokenType}.");
        }

        var stringValue = reader.GetString();

        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return null;
        }

        // Check if the string contains timezone information (Z, +HH:MM, -HH:MM after the date/time portion)
        if (HasTimezoneInfo(stringValue))
        {
            if (DateTimeOffset.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
            {
                return dto;
            }

            throw new JsonException($"Invalid {nameof(DateTimeOffset)} value: '{stringValue}'.");
        }

        // Parse without timezone info, assuming UTC. In practice, the Azure Resource Health API
        // only omits the timezone designator for sentinel values like "0001-01-01T00:00:00"
        // (DateTime.MinValue) representing unset fields (e.g., impactMitigationTime for
        // active/unmitigated events). Real timestamps always include the "Z" suffix.
        if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dt))
        {
            return new DateTimeOffset(dt, TimeSpan.Zero);
        }

        throw new JsonException($"Invalid DateTime value: '{stringValue}'.");
    }

    private static bool HasTimezoneInfo(string value)
    {
        // Check for UTC indicator or offset at end of ISO 8601 datetime strings
        // e.g. "2025-03-01T10:00:00Z", "2025-03-01T10:00:00+00:00", "2025-03-01T10:00:00-08:00"
        return value.EndsWith('Z') ||
               value.Length > 6 && (value[^6] == '+' || value[^6] == '-');
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
