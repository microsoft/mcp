// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Areas.Server.Models;

/// <summary>
/// Represents the root structure of the MCP server registry JSON file.
/// Contains a collection of server configurations keyed by server name.
/// </summary>
public interface IRegistryRoot
{
    /// <summary>
    /// Gets the dictionary of server configurations, keyed by server name.
    /// </summary>
    Dictionary<string, RegistryServerInfo>? Servers { get; init; }
}
