// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Messaging.ServiceBus.Administration;

namespace Azure.Mcp.Tools.ServiceBus.Models.utilities;

public class EntityStatusConverter : JsonConverter<EntityStatus>
{
    public override EntityStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new EntityStatus(reader.GetString());
        }
        throw new JsonException($"Unexpected token type {reader.TokenType} for EntityStatus.");
    }

    public override void Write(Utf8JsonWriter writer, EntityStatus value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
