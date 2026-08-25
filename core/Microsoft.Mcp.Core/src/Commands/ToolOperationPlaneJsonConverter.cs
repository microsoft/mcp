// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Mcp.Core.Commands;

public sealed class ToolOperationPlaneJsonConverter : JsonConverter<ToolOperationPlane>
{
    public override ToolOperationPlane Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String ||
            !ToolOperationPlaneExtensions.TryParseJsonValue(reader.GetString(), out var operationPlane))
        {
            throw new JsonException("The tool operation plane must be one of: unspecified, data, control, both, or notApplicable.");
        }

        return operationPlane;
    }

    public override void Write(Utf8JsonWriter writer, ToolOperationPlane value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToJsonValue());
}
