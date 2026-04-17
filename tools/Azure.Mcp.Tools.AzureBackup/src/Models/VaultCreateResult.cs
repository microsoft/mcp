// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record VaultCreateResult(
    string? Id,
    string Name,
    string VaultType,
    string? Location,
    string? ProvisioningState);
