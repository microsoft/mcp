// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Areas.Server;
using Microsoft.Mcp.Core.Models.Metadata;

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Custom JSON converter for <see cref="ToolMetadata"/> that handles serialization and deserialization
/// of metadata properties with nested value and description objects.
/// </summary>
public sealed class ToolMetadataConverter : JsonConverter<ToolMetadata>
{
    /// <summary>
    /// Stable serialized values for <see cref="ToolOperationPlane"/>. Writing is strict so an enum
    /// value added without a serialized form fails loudly instead of silently emitting
    /// <c>unspecified</c>; reading is lenient so newer metadata remains readable by older binaries.
    /// </summary>
    private static string ToJsonValue(ToolOperationPlane operationPlane) => operationPlane switch
    {
        ToolOperationPlane.Unspecified => "unspecified",
        ToolOperationPlane.Data => "data",
        ToolOperationPlane.Control => "control",
        ToolOperationPlane.Both => "both",
        ToolOperationPlane.NotApplicable => "notApplicable",
        _ => throw new JsonException($"'{operationPlane}' has no serialized operation plane value.")
    };

    private static ToolOperationPlane FromJsonValue(string? value) => value switch
    {
        "data" => ToolOperationPlane.Data,
        "control" => ToolOperationPlane.Control,
        "both" => ToolOperationPlane.Both,
        "notApplicable" => ToolOperationPlane.NotApplicable,
        _ => ToolOperationPlane.Unspecified
    };

    public override ToolMetadata Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        MetadataDefinition GetMetadata(string name, bool defaultValue)
        {
            if (!root.TryGetProperty(name, out var prop))
                return new MetadataDefinition { Value = defaultValue, Description = string.Empty };

            var meta = JsonSerializer.Deserialize(prop.GetRawText(), CoreJsonContext.Default.MetadataDefinition)
                ?? new MetadataDefinition { Value = defaultValue, Description = string.Empty };
            return meta;
        }

        ToolOperationPlane GetOperationPlane()
            => root.TryGetProperty("operationPlane", out var property) && property.ValueKind == JsonValueKind.String
                ? FromJsonValue(property.GetString())
                : ToolOperationPlane.Unspecified;

        return new ToolMetadata(
            GetOperationPlane(),
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
            JsonSerializer.Serialize(writer, def, CoreJsonContext.Default.MetadataDefinition);
        }

        writer.WriteString("operationPlane", ToJsonValue(value.OperationPlane));
        WriteMetadata("destructive", value.DestructiveProperty);
        WriteMetadata("idempotent", value.IdempotentProperty);
        WriteMetadata("openWorld", value.OpenWorldProperty);
        WriteMetadata("readOnly", value.ReadOnlyProperty);
        WriteMetadata("secret", value.SecretProperty);
        WriteMetadata("localRequired", value.LocalRequiredProperty);

        writer.WriteEndObject();
    }

}
