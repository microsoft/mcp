// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Tools.Options;

public sealed class ToolsListNamesOptions
{
    /// <summary>
    /// Optional namespace to filter tool names. If provided, only tools from this namespace will be returned.
    /// </summary>
    public string? Namespace { get; set; }
}