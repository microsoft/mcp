// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTOperations.Models;

/// <summary>
/// Lightweight projection of an Azure IoT Operations Instance resource
/// (Microsoft.IoTOperations/instances).
/// </summary>
public sealed record IoTOperationsInstanceInfo(
    string Name,
    string? Id,
    string? Location,
    string? ResourceGroup,
    string? Type,
    string? ProvisioningState,
    string? Version,
    string? Description,
    string? SchemaRegistryResourceId);
