// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;

namespace Azure.Mcp.Tools.SignalR.Models;

/// <summary>
/// Represents managed identity configuration of the SignalR resource.
/// </summary>
public sealed class Identity
{
    /// <summary> The identity type. </summary>
    public string? Type { get; set; }

    /// <summary> Details of the managed identity. </summary>
    public ManagedIdentityInfo? ManagedIdentityInfo { get; set; }
}
