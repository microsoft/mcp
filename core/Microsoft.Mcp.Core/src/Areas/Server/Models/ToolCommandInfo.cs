// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

public sealed class ToolCommandInfo
{
    public string Name { get; init; }
    public string? Description { get; init; }
    public JsonElement? Properties { get; init; }
    public JsonElement? Required { get; init; }

    public ToolCommandInfo(Tool tool, bool includeSchema = true)
    {
        Name = tool.Name;
        Description = tool.Description;
        if (includeSchema)
        {
            if (tool.InputSchema.TryGetProperty("properties", out var properties))
            {
                Properties = properties;
            }
            if (tool.InputSchema.TryGetProperty("required", out var required))
            {
                Required = required;
            }
        }
    }
}
