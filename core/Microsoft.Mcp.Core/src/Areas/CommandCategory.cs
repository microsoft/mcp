// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas;

public enum CommandCategory
{
    /// <summary>
    /// Commands related to the CLI.
    /// </summary>
    Cli,

    /// <summary>
    /// Commands related to the MCP server.
    /// </summary>
    Mcp,

    /// <summary>
    /// Commands related to best practices.
    /// </summary>
    RecommendedTools,

    /// <summary>
    /// Commands related to subscription and groups.
    /// </summary>
    SubscriptionManagement,

    /// <summary>
    /// Commands related to all Azure Services tools.
    /// </summary>
    AzureServices
}
