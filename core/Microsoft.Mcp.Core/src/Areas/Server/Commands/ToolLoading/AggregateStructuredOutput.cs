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
            && rootObject[StructuredOutputJson.ToolsPropertyName] is JsonNode nestedTools
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
                [StructuredOutputJson.MessagePropertyName] = response.Message
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
            [StructuredOutputJson.KindPropertyName] = StructuredOutputJson.MessageKind,
            [StructuredOutputJson.MessagePropertyName] = message
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
            [StructuredOutputJson.KindPropertyName] = StructuredOutputJson.ToolListKind,
            [StructuredOutputJson.ToolsPropertyName] = tools.DeepClone()
        };
        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement BuildToolResult(string? tool, string command, JsonNode? result)
    {
        var envelope = new JsonObject
        {
            [StructuredOutputJson.KindPropertyName] = StructuredOutputJson.ToolResultKind,
            [StructuredOutputJson.CommandPropertyName] = command,
            [StructuredOutputJson.ResultPropertyName] = result?.DeepClone()
        };

        if (tool is not null)
        {
            envelope[StructuredOutputJson.ToolPropertyName] = tool;
        }

        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement CreateOutputSchema(bool requireTool)
    {
        var toolResultProperties = new JsonObject
        {
            [StructuredOutputJson.KindPropertyName] = new JsonObject { ["const"] = StructuredOutputJson.ToolResultKind },
            [StructuredOutputJson.CommandPropertyName] = new JsonObject { ["type"] = "string" },
            [StructuredOutputJson.ResultPropertyName] = new JsonObject()
        };
        var toolResultRequired = new JsonArray(
            StructuredOutputJson.KindPropertyName,
            StructuredOutputJson.CommandPropertyName,
            StructuredOutputJson.ResultPropertyName);

        if (requireTool)
        {
            toolResultProperties[StructuredOutputJson.ToolPropertyName] = new JsonObject { ["type"] = "string" };
            toolResultProperties[StructuredOutputJson.ResultPropertyName] = new JsonObject { ["type"] = "object" };
            toolResultRequired.Insert(1, StructuredOutputJson.ToolPropertyName);
        }

        var toolListSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                [StructuredOutputJson.KindPropertyName] = new JsonObject { ["const"] = StructuredOutputJson.ToolListKind },
                [StructuredOutputJson.ToolsPropertyName] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "object" }
                }
            },
            ["required"] = new JsonArray(StructuredOutputJson.KindPropertyName, StructuredOutputJson.ToolsPropertyName),
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
                [StructuredOutputJson.KindPropertyName] = new JsonObject { ["const"] = StructuredOutputJson.MessageKind },
                [StructuredOutputJson.MessagePropertyName] = new JsonObject { ["type"] = "string" }
            },
            ["required"] = new JsonArray(StructuredOutputJson.KindPropertyName, StructuredOutputJson.MessagePropertyName),
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
