// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Options;

/// <summary>
/// Defines the supported proxy modes for the Azure MCP server.
/// </summary>
internal static class ModeTypes
{
    /// <summary>
    /// Single tool proxy mode - exposes a single "azure" tool that handles internal routing across all Azure MCP tools.
    /// </summary>
    public const string SingleToolProxy = "single";

    /// <summary>
    /// Namespace tool proxy mode - collapses all tools within each namespace into a single tool
    /// (e.g., all storage operations become one "storage" tool with internal routing).
    /// </summary>
    public const string NamespaceProxy = "namespace";

    /// <summary>
    /// All tools mode - exposes all Azure MCP tools individually (one tool per command).
    /// </summary>
    public const string All = "all";
}
