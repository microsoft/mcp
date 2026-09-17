// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesVolume(
    string Name,
    string Id,
    string Location,
    string? ProvisioningState,
    long? QuotaGib,
    string? ServiceLevel);