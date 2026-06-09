// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Models;

public sealed record VmUpdateResult(
    string Name,
    string? Id,
    string? Location,
    string? VmSize,
    string? ProvisioningState,
    string? PowerState,
    string? OsType,
    string? LicenseType,
    IReadOnlyList<string>? Zones,
    IReadOnlyDictionary<string, string>? Tags);
