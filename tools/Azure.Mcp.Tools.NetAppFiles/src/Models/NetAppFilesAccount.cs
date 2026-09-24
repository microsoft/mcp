// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.NetAppFiles.Models;

public record NetAppFilesAccount(
    string Name,
    string Id,
    string Location,
    string? ProvisioningState);
