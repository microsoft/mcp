// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Core.Areas.Server.Commands.Discovery;

/// <summary>
/// Constants used by discovery strategies and tool loaders.
/// </summary>
public static class DiscoveryConstants
{
    /// <summary>
    /// Utility namespaces that should be exposed as individual commands rather than namespace proxies.
    /// These commands are always available in namespace mode regardless of namespace filters.
    /// </summary>
    public static readonly string[] UtilityNamespaces = ["subscription", "group"];

    /// <summary>
    /// Command groups that should be ignored when discovering namespace proxy servers.
    /// These include both core infrastructure groups and utility groups that are handled separately.
    /// </summary>
    public static readonly string[] IgnoredCommandGroups = ["extension", "server", "tools", .. UtilityNamespaces];
}
