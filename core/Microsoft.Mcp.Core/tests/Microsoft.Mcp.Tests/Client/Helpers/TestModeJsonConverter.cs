// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Mcp.Tests.Helpers;

namespace Microsoft.Mcp.Tests.Client.Helpers;

public sealed class TestModeJsonConverter() : JsonConverter<TestMode>
{
    private static readonly string s_validValues = string.Join(", ", Enum.GetNames<TestMode>());

    public override TestMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Invalid TestMode value. TestMode must be one of: {s_validValues}. Token type was {reader.TokenType}.");
        }

        var value = reader.GetString();

        if (!string.IsNullOrWhiteSpace(value) &&
            Enum.TryParse<TestMode>(value, ignoreCase: true, out var testMode) &&
            Enum.IsDefined(typeof(TestMode), testMode) &&
            !int.TryParse(value, out _))
        {
            return testMode;
        }

        throw new JsonException($"Invalid TestMode '{value}'. TestMode must be one of: {s_validValues}.");
    }

    public override void Write(Utf8JsonWriter writer, TestMode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}