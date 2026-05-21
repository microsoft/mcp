// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using ModelContextProtocol.Protocol;

namespace Microsoft.Mcp.Core.Areas.Server.Models;

public sealed class ToolCommandInfo(Tool tool)
{
    public string Name { get; } = tool.Name;
    public string? Description { get; } = tool.Description;
    public JsonElement Properties { get; } = tool.InputSchema.GetProperty("properties");
    public JsonElement Required { get; } = tool.InputSchema.GetProperty("required");
}
