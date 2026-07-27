// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal static class AggregateStructuredOutput
{
    internal static JsonElement NamespaceOutputSchema { get; } = CreateOutputSchema(requireTool: false);

    internal static JsonElement SingleOutputSchema { get; } = CreateOutputSchema(requireTool: true);

    internal static JsonElement CreateToolList(IEnumerable<Tool> tools)
    {
        var toolsNode = JsonSerializer.SerializeToNode(tools, ServerJsonContext.Default.IEnumerableTool);
        return CreateToolList(toolsNode);
    }

    internal static JsonElement CreateToolList(string toolsJson)
    {
        var root = JsonNode.Parse(toolsJson)
            ?? throw new JsonException("The serialized tool list was empty.");
        var toolsNode = root is JsonObject rootObject && rootObject["tools"] is JsonNode nestedTools
            ? nestedTools
            : root;

        return CreateToolList(toolsNode);
    }

    internal static JsonElement CreateToolResult(string command, string jsonResponse)
    {
        using var document = JsonDocument.Parse(jsonResponse);
        JsonNode? result = null;
        if (document.RootElement.TryGetProperty("results", out var results))
        {
            result = JsonNode.Parse(results.GetRawText());
        }

        return BuildToolResult(tool: null, command, result);
    }

    internal static JsonElement CreateToolResult(string tool, string command, JsonNode? result) =>
        BuildToolResult(tool, command, result);

    internal static JsonElement CreateMessage(string message)
    {
        var envelope = new JsonObject
        {
            ["kind"] = "message",
            ["message"] = message
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
            ["kind"] = "tool-list",
            ["tools"] = tools.DeepClone()
        };
        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement BuildToolResult(string? tool, string command, JsonNode? result)
    {
        var envelope = new JsonObject
        {
            ["kind"] = "tool-result",
            ["command"] = command,
            ["result"] = result?.DeepClone()
        };

        if (tool is not null)
        {
            envelope["tool"] = tool;
        }

        return JsonSerializer.SerializeToElement(envelope, ServerJsonContext.Default.JsonObject);
    }

    private static JsonElement CreateOutputSchema(bool requireTool)
    {
        var toolResultProperties = new JsonObject
        {
            ["kind"] = new JsonObject { ["const"] = "tool-result" },
            ["command"] = new JsonObject { ["type"] = "string" },
            ["result"] = new JsonObject()
        };
        var toolResultRequired = new JsonArray("kind", "command", "result");

        if (requireTool)
        {
            toolResultProperties["tool"] = new JsonObject { ["type"] = "string" };
            toolResultProperties["result"] = new JsonObject { ["type"] = "object" };
            toolResultRequired.Insert(1, "tool");
        }

        var toolListSchema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["kind"] = new JsonObject { ["const"] = "tool-list" },
                ["tools"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "object" }
                }
            },
            ["required"] = new JsonArray("kind", "tools"),
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
                ["kind"] = new JsonObject { ["const"] = "message" },
                ["message"] = new JsonObject { ["type"] = "string" }
            },
            ["required"] = new JsonArray("kind", "message"),
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
