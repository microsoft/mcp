// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.CloudArchitect.Options;

/// <summary>
/// AOT-safe JSON converter for RequirementImportance enum that handles case-insensitive deserialization.
/// </summary>
public sealed class RequirementImportanceConverter : JsonConverter<RequirementImportance>
{
    public override RequirementImportance Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Expected string token, got {reader.TokenType}");
        }

        var value = reader.GetString();
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException("Null or empty string is not a valid RequirementImportance value");
        }

        return value.ToLowerInvariant() switch
        {
            "high" => RequirementImportance.High,
            "critical" => RequirementImportance.High, // Treat "critical" as "high" importance
            "medium" => RequirementImportance.Medium,
            "low" => RequirementImportance.Low,
            _ => throw new JsonException($"'{value}' is not a valid RequirementImportance value")
        };
    }

    public override void Write(Utf8JsonWriter writer, RequirementImportance value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
