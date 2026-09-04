// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Mcp.Tests.Helpers;

namespace Microsoft.Mcp.Tests.Client.Helpers;

public sealed class TestModeJsonConverter() : JsonConverter<TestMode>
{
    private const string ValidValues = "Live, Record, Playback";

    public override TestMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"Invalid TestMode value. TestMode must be one of: {ValidValues}.");
        }

        var value = reader.GetString();

        if (Enum.TryParse<TestMode>(value, true, out var testMode) &&
            Enum.IsDefined(testMode) &&
            !int.TryParse(value, out _))
        {
            return testMode;
        }

        throw new JsonException($"Invalid TestMode '{value}'. TestMode must be one of: {ValidValues}.");
    }

    public override void Write(Utf8JsonWriter writer, TestMode value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}