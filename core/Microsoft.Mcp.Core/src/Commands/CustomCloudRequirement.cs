// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Commands;

/// <summary>
/// Describes the custom-cloud capability required to expose a tool.
/// </summary>
public enum CustomCloudRequirement
{
    /// <summary>
    /// The tool does not require an optional custom-cloud capability.
    /// </summary>
    None,

    /// <summary>
    /// The tool is unavailable in custom clouds.
    /// </summary>
    Unsupported,

    /// <summary>
    /// The tool requires the Log Analytics custom-cloud capability.
    /// </summary>
    LogAnalytics,

    /// <summary>
    /// The tool requires the Kusto custom-cloud capability.
    /// </summary>
    Kusto
}
