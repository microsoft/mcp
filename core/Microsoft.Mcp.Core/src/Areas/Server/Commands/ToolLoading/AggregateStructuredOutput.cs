// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal static class AggregateStructuredOutput
{
    internal static JsonElement NamespaceOutputSchema { get; } = CreateOutputSchema(requireTool: false);

    internal static JsonElement SingleOutputSchema { get; } = CreateOutputSchema(requireTool: true);

    internal static JsonElement CreateToolList(string toolsJson)
    {
        var root = JsonNode.Parse(toolsJson)
            ?? throw new JsonException("The serialized tool list was empty.");
        var toolsNode = root is JsonObject rootObject
            && rootObject[StructuredOutputHelper.ToolsPropertyName] is JsonNode nestedTools
            ? nestedTools
            : root;

        return CreateToolList(toolsNode);
    }

    internal static JsonElement? CreateToolResult(string command, CommandResponse response)
    {
        var result = response.Results?.ToJsonNode();
        if (result is null)
        {
            if (string.IsNullOrEmpty(response.Message))
            {
                return null;
            }

            result = new JsonObject
            {
                [StructuredOutputHelper.MessagePropertyName] = response.Message
            };
        }

        return BuildToolResult(tool: null, command, result);
    }

    internal static JsonElement CreateToolResult(string tool, string command, JsonNode? result) =>
        BuildToolResult(tool, command, result);

    internal static JsonElement CreateMessage(string message)
    {
        var envelope = new JsonObject
        {
            [StructuredOutputHelper.KindPropertyName] = StructuredOutputHelper.MessageKind,
            [StructuredOutputHelper.MessagePropertyName] = message
        };
        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement CreateToolList(JsonNode? tools)
    {
        if (tools is not JsonArray)
        {
            throw new JsonException("The serialized tool list must be a JSON array.");
        }

        var envelope = new JsonObject
        {
            [StructuredOutputHelper.KindPropertyName] = StructuredOutputHelper.ToolListKind,
            [StructuredOutputHelper.ToolsPropertyName] = tools.DeepClone()
        };
        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement BuildToolResult(string? tool, string command, JsonNode? result)
    {
        var envelope = new JsonObject
        {
            [StructuredOutputHelper.KindPropertyName] = StructuredOutputHelper.ToolResultKind,
            [StructuredOutputHelper.CommandPropertyName] = command,
            [StructuredOutputHelper.ResultPropertyName] = result?.DeepClone()
        };

        if (tool is not null)
        {
            envelope[StructuredOutputHelper.ToolPropertyName] = tool;
        }

        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement CreateOutputSchema(bool requireTool)
    {
        var toolResultProperties = new JsonObject
        {
            [StructuredOutputHelper.KindPropertyName] = new JsonObject { ["const"] = StructuredOutputHelper.ToolResultKind },
            [StructuredOutputHelper.CommandPropertyName] = new JsonObject { ["type"] = "string" },
            [StructuredOutputHelper.ResultPropertyName] = new JsonObject()
        };
        var toolResultRequired = new JsonArray(
            StructuredOutputHelper.KindPropertyName,
            StructuredOutputHelper.CommandPropertyName,
            StructuredOutputHelper.ResultPropertyName);

        if (requireTool)
        {
            toolResultProperties[StructuredOutputHelper.ToolPropertyName] = new JsonObject { ["type"] = "string" };
            toolResultProperties[StructuredOutputHelper.ResultPropertyName] = new JsonObject { ["type"] = "object" };
            toolResultRequired.Insert(1, StructuredOutputHelper.ToolPropertyName);
        }

        var toolListSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [StructuredOutputHelper.KindPropertyName] = new JsonObject { ["const"] = StructuredOutputHelper.ToolListKind },
                [StructuredOutputHelper.ToolsPropertyName] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "object" }
                }
            },
            ["required"] = new JsonArray(StructuredOutputHelper.KindPropertyName, StructuredOutputHelper.ToolsPropertyName),
            ["additionalProperties"] = false
        };
        var toolResultSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = toolResultProperties,
            ["required"] = toolResultRequired,
            ["additionalProperties"] = false
        };
        var messageSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [StructuredOutputHelper.KindPropertyName] = new JsonObject { ["const"] = StructuredOutputHelper.MessageKind },
                [StructuredOutputHelper.MessagePropertyName] = new JsonObject { ["type"] = "string" }
            },
            ["required"] = new JsonArray(StructuredOutputHelper.KindPropertyName, StructuredOutputHelper.MessagePropertyName),
            ["additionalProperties"] = false
        };
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["oneOf"] = new JsonArray(toolListSchema, toolResultSchema, messageSchema)
        };

        return JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.JsonObject);
    }
}
