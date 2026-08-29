// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Options;
using Microsoft.Mcp.Core.Models.Command;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal static class StructuredOutputHelper
{
    internal const string CompactContentMessage = "Response successful. See structuredContent for details.";

    /// <summary>
    /// Extracts and shapes a command result payload.
    /// Object payloads are used as-is; arrays and scalars are wrapped under <c>value</c>.
    /// </summary>
    internal static JsonElement? TryBuildStructuredContent(ResponseResult? result)
    {
        var resultNode = result?.ToJsonNode();
        if (resultNode is null)
        {
            return null;
        }

        var structuredContent = resultNode as JsonObject
            ?? StructuredOutputJson.WrapValue(resultNode);
        return JsonSerializer.SerializeToElement(structuredContent, ServerJsonContext.Default.JsonObject);
    }

    internal static CallToolResult CreateCallToolResult(
        StructuredOutputMode? mode,
        Func<string> contentFactory,
        Func<JsonElement?>? structuredContentFactory = null,
        bool? isError = null)
    {
        var structuredContent = mode is not null && isError != true
            ? structuredContentFactory?.Invoke()
            : null;
        var contentText = structuredContent.HasValue && mode == StructuredOutputMode.Compact
            ? CompactContentMessage
            : contentFactory();

        return new CallToolResult
        {
            Content = [new TextContentBlock { Text = contentText }],
            StructuredContent = structuredContent,
            IsError = isError
        };
    }
}
