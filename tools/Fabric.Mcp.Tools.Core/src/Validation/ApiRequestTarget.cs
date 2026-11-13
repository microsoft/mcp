// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Validation;

/// <summary>One allowlisted REST API: its fixed host, required path prefix, and token scopes.</summary>
internal sealed record ApiRequestTarget(string Name, string Host, string PathPrefix, string[] Scopes)
{
    internal const string FabricName = "fabric";
    internal const string PowerBIName = "powerbi";

    internal static ApiRequestTarget Fabric { get; } =
        new(FabricName, FabricEndpoints.FabricApiHost, "/v1/", FabricEndpoints.FabricScopes);

    internal static ApiRequestTarget PowerBI { get; } =
        new(PowerBIName, FabricEndpoints.PowerBIApiHost, "/v1.0/myorg/", FabricEndpoints.PowerBIScopes);

    internal static ApiRequestTarget? Find(string? name) => name?.Trim().ToLowerInvariant() switch
    {
        null or "" or FabricName => Fabric,
        PowerBIName => PowerBI,
        _ => null
    };
}
