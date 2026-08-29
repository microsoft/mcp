// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Models.Command;

namespace Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;

internal static class StructuredOutputHelper
{
    internal const string CompactContentMessage = "Response successful. See structuredContent for details.";

    /// <summary>
    /// Extracts and shapes the result payload from a serialized <see cref="CommandResponse"/>.
    /// Object payloads are used as-is; arrays and scalars are wrapped under <c>value</c>.
    /// </summary>
    internal static JsonElement? TryBuildStructuredContent(string jsonResponse)
    {
        using var document = JsonDocument.Parse(jsonResponse);

        if (!document.RootElement.TryGetProperty(StructuredOutputJson.ResultsPropertyName, out var results))
        {
            return null;
        }

        if (StructuredOutputJson.IsObjectRoot(results))
        {
            return results.Clone();
        }

        if (results.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        var wrapper = StructuredOutputJson.WrapValue(JsonNode.Parse(results.GetRawText()));
        return JsonSerializer.SerializeToElement(wrapper, ServerJsonContext.Default.JsonObject);
    }
}
