// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Acr.Services.Models;

/// <summary>
/// A class representing the ContainerRegistry properties data model.
/// </summary>
internal sealed class ContainerRegistryProperties
{
    /// <summary> The URL that can be used to log into the container registry. </summary>
    public string? LoginServer { get; set; }
    /// <summary> The provisioning state of the container registry at the time the operation was called. </summary>
    public string? ProvisioningState { get; set; }
}
