// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ContainerApps.Models;

public sealed record ContainerAppInfo(
    string Name,
    string? Location,
    string? ResourceGroup,
    string? ManagedEnvironmentId,
    string? ProvisioningState);
