// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Tools.Options;

public sealed class ToolsListOptions
{
    public bool NamespaceMode { get; set; } = false;

    /// <summary>
    /// If true, returns only tool names without descriptions or metadata.
    /// </summary>
    public bool NameOnly { get; set; } = false;

    /// <summary>
    /// Optional namespaces to filter tools. If provided, only tools from these namespaces will be returned.
    /// </summary>
    public List<string> Namespaces { get; set; } = new();
}
