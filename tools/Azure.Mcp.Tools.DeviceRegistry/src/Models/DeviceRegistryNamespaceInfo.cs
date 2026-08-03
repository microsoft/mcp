// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceRegistry.Models;

/// <summary>
/// Lightweight projection of a Device Registry Namespace resource.
/// </summary>
public sealed record DeviceRegistryNamespaceInfo(
    string Name,
    string? Id,
    string? Location,
    string? ProvisioningState,
    string? Uuid,
    string? ResourceGroup,
    string? Type);
