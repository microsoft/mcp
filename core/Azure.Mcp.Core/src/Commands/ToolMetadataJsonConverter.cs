using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas.Server;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Metadata;

public sealed class ToolMetadataConverter : JsonConverter<ToolMetadata>
{
    public override ToolMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        MetadataDefinition GetMetadata(string name, bool defaultValue)
        {
            if (!root.TryGetProperty(name, out var prop))
                return new MetadataDefinition { Value = defaultValue, Description = string.Empty };

            var meta = JsonSerializer.Deserialize(prop.GetRawText(), ServerJsonContext.Default.MetadataDefinition)
                       ?? new MetadataDefinition { Value = defaultValue, Description = string.Empty };
            return meta;
        }
        return new ToolMetadata(
            GetMetadata("destructive", true),
            GetMetadata("idempotent", false),
            GetMetadata("openWorld", true),
            GetMetadata("readOnly", false),
            GetMetadata("secret", false),
            GetMetadata("localRequired", false)
        );
    }

    public override void Write(Utf8JsonWriter writer, ToolMetadata value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        void WriteMetadata(string name, MetadataDefinition def)
        {
            writer.WritePropertyName(name);
            JsonSerializer.Serialize(writer, def, ServerJsonContext.Default.MetadataDefinition);
        }

        WriteMetadata("destructive", value.DestructiveProperty);
        WriteMetadata("idempotent", value.IdempotentProperty);
        WriteMetadata("openWorld", value.OpenWorldProperty);
        WriteMetadata("readOnly", value.ReadOnlyProperty);
        WriteMetadata("secret", value.SecretProperty);
        WriteMetadata("localRequired", value.LocalRequiredProperty);

        writer.WriteEndObject();
    }

}
