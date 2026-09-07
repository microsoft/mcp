// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

public sealed record ProtectedItemDppIdentityDetails(
    string? UserAssignedIdentityArmUri,
    bool? UseSystemAssignedIdentity,
    string? UserAssignedIdentityId);