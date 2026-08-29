// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

internal static class StructuredOutputJson
{
    internal const string CommandPropertyName = "command";
    internal const string KindPropertyName = "kind";
    internal const string MessagePropertyName = "message";
    internal const string ResultPropertyName = "result";
    internal const string ResultsPropertyName = "results";
    internal const string ToolPropertyName = "tool";
    internal const string ToolsPropertyName = "tools";
    internal const string ValuePropertyName = "value";

    internal const string MessageKind = "message";
    internal const string ToolListKind = "tool-list";
    internal const string ToolResultKind = "tool-result";

    internal static bool IsObjectRoot(JsonObject schema) =>
        schema["type"] is JsonValue typeValue
        && typeValue.TryGetValue<string>(out var typeName)
        && typeName == "object";

    internal static bool IsObjectRoot(JsonElement value) => value.ValueKind == JsonValueKind.Object;

    internal static JsonObject WrapValue(JsonNode? value) =>
        new() { [ValuePropertyName] = value?.DeepClone() };
}
