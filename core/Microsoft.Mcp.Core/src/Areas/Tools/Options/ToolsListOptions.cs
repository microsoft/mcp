// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Microsoft.Mcp.Core.Areas.Tools.Options;

public sealed class ToolsListOptions
{
    [Option(Description = "If specified, returns a list of top-level service namespaces instead of individual tools.")]
    public bool NamespaceMode { get; set; } = false;

    /// <summary>
    /// If true, returns only tool names without descriptions or metadata.
    /// </summary>
    [Option(Description = "If specified, returns only tool names without descriptions or metadata.")]
    public bool NameOnly { get; set; } = false;

    /// <summary>
    /// Optional namespaces to filter tools. If provided, only tools from these namespaces will be returned.
    /// </summary>
    [Option(Description = "Filter tools by namespace (e.g., 'storage', 'keyvault'). Can be specified multiple times to include multiple namespaces.")]
    public string[]? Namespace { get; set; }
}
