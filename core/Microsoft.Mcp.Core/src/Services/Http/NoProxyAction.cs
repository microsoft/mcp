// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Microsoft.Mcp.Core.Services.Http;

/// <summary>
/// Specifies how requests excluded from an explicit proxy by <c>NO_PROXY</c> are routed.
/// </summary>
public enum NoProxyAction
{
    /// <summary>
    /// Connect directly through namespace-aware transport IP address filtering.
    /// </summary>
    DirectWithIpFiltering,

    /// <summary>
    /// Connect directly without transport IP address filtering.
    /// </summary>
    DirectDangerouslyWithoutIpFiltering
}
