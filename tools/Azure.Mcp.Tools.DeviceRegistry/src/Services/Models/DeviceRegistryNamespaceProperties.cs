// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceRegistry.Services.Models;

/// <summary>
/// Properties of a Device Registry Namespace.
/// </summary>
internal sealed class DeviceRegistryNamespaceProperties
{
    /// <summary> The provisioning state of the namespace. </summary>
    public string? ProvisioningState { get; set; }

    /// <summary> The unique identifier of the namespace. </summary>
    public string? Uuid { get; set; }
}
