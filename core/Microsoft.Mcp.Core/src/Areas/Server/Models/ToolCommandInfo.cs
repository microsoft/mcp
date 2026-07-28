// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

public sealed class ToolCommandInfo
{
    /// <summary>
    /// Reference to the <see cref="Tool"/>'s Name. Used when sampling for tool area selection in --mode all's outer sampling request.
    /// </summary>
    public string? Tool { get; init; }

    /// <summary>
    /// Reference to the <see cref="Tool"/>'s Name. Used when sampling for tool command selection in --mode all's inner sampling request
    /// or --mode namespace's sampling request.
    /// </summary>
    public string? Command { get; init; }

    /// <summary>
    /// Description of the tool.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Properties of the tool's input schema.
    /// </summary>
    public JsonElement? Properties { get; init; }

    /// <summary>
    /// Required properties of the tool's input schema.
    /// </summary>
    public JsonElement? Required { get; init; }

    public ToolCommandInfo(Tool tool, bool includeSchema = true)
    {
        Description = tool.Description;
        if (includeSchema)
        {
            Command = tool.Name;
            if (tool.InputSchema.TryGetProperty("properties", out var properties))
            {
                Properties = properties;
            }
            if (tool.InputSchema.TryGetProperty("required", out var required))
            {
                Required = required;
            }
        }
        else
        {
            Tool = tool.Name;
        }
    }
}
