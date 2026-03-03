// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Mcp.Core.Areas.Server.Models;
using Microsoft.Mcp.Core.Helpers;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Commands;

/// <summary>
/// Generates a <see cref="ToolInputSchema"/> from a command's <see cref="Option"/> list,
/// reflecting the shape of a command's input parameters for use as <c>inputSchema</c>
/// on MCP tool definitions.
/// </summary>
public static class InputSchemaGenerator
{
    /// <summary>
    /// The well-known option name used to pass raw MCP tool input as a single JSON object.
    /// </summary>
    public const string RawMcpToolInputOptionName = "raw-mcp-tool-input";

    /// <summary>
    /// Generates a JSON Schema element from the given command options.
    /// Options are mapped to schema properties with their types and descriptions.
    /// Required options are listed in the schema's <c>required</c> array.
    /// </summary>
    /// <param name="options">The command options to generate a schema from. May be null or empty.</param>
    /// <returns>A <see cref="JsonElement"/> representing the input JSON schema.</returns>
    public static JsonElement Generate(IList<Option>? options)
    {
        if (options is { Count: > 0 })
        {
            // Special case: a single raw MCP tool input option carries its own schema
            if (options.Count == 1 && IsRawMcpToolInputOption(options[0]))
            {
                var arguments = JsonNode.Parse(options[0].Description ?? "{}") as JsonObject ?? new JsonObject();
                return JsonSerializer.SerializeToElement(arguments, ServerJsonContext.Default.JsonObject);
            }

            var schema = new ToolInputSchema();

            foreach (var option in options)
            {
                var propName = NameNormalization.NormalizeOptionName(option.Name);
                schema.Properties.Add(propName, TypeToJsonTypeMapper.CreatePropertySchema(option.ValueType, option.Description));
            }

            schema.Required = [.. options.Where(p => p.Required).Select(p => NameNormalization.NormalizeOptionName(p.Name))];

            return JsonSerializer.SerializeToElement(schema, ServerJsonContext.Default.ToolInputSchema);
        }

        return JsonSerializer.SerializeToElement(new ToolInputSchema(), ServerJsonContext.Default.ToolInputSchema);
    }

    /// <summary>
    /// Determines whether the given option is a raw MCP tool input option.
    /// Raw MCP tool input options carry their own JSON schema in the description field
    /// and bypass normal option-to-property mapping.
    /// </summary>
    /// <param name="option">The option to check.</param>
    /// <returns><c>true</c> if the option is a raw MCP tool input option; otherwise <c>false</c>.</returns>
    public static bool IsRawMcpToolInputOption(Option option)
    {
        if (string.Equals(NameNormalization.NormalizeOptionName(option.Name), RawMcpToolInputOptionName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var alias in option.Aliases)
        {
            if (string.Equals(NameNormalization.NormalizeOptionName(alias), RawMcpToolInputOptionName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
