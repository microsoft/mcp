// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ManagedLustre.Options.FileSystem;

public sealed class FileSystemCreateOptions : ISubscriptionOption
{
    [Option(Description = ManagedLustreOptionDescriptions.Name)]
    public required string Name { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.Location)]
    public required string Location { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.Sku)]
    public required string Sku { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.Size)]
    public required int Size { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.SubnetId)]
    public required string SubnetId { get; set; }

    [Option(Description = "Availability zone identifier. Use a single digit string matching the region's AZ labels (e.g. '1'). Example: --zone 1")]
    public required string Zone { get; set; }

    [Option(Description = "Full blob container resource ID for HSM integration. HPC Cache Resource Provider must have before deployment Storage Blob Data Contributor and Storage Account Contributor roles on parent Storage Account. " +
        "Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Storage/storageAccounts/{account}/blobServices/default/containers/{container}. " +
        "Example: --hsm-container /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stacc/blobServices/default/containers/hsm-container")]
    public string? HsmContainer { get; set; }

    [Option(Description = "Full blob container resource ID for HSM logging. HPC Cache Resource Provider must have before deployment Storage Blob Data Contributor and Storage Account Contributor roles on parent Storage Account. Same format as --hsm-container. " +
        "Example: --hsm-log-container /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stacc/blobServices/default/containers/hsm-logs")]
    public string? HsmLogContainer { get; set; }

    [Option(Description = "Optional HSM import prefix (path prefix inside the container starting with /). Examples: '/ingest/', '/archive/2019/'.")]
    public string? ImportPrefix { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.MaintenanceDay)]
    public required string MaintenanceDay { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.MaintenanceTime)]
    public required string MaintenanceTime { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.RootSquashMode)]
    public string? RootSquashMode { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.NoSquashNidLists)]
    public string? NoSquashNidList { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.SquashUid)]
    public long? SquashUid { get; set; }

    [Option(Description = ManagedLustreOptionDescriptions.SquashGid)]
    public long? SquashGid { get; set; }

    [Option(Description = "Enable customer-managed encryption using a Key Vault key. When true, --key-url and --source-vault required, with a user-assigned identity already configured for Key Vault key access.")]
    public bool? CustomEncryption { get; set; }

    [Option(Description = "Full Key Vault key URL. Format: https://{vaultName}.vault.azure.net/keys/{keyName}/{keyVersion}. " +
        "Example: --key-url https://kv-amlfs-001.vault.azure.net/keys/key-amlfs-001/a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p")]
    public string? KeyUrl { get; set; }

    [Option(Description = "Full Key Vault resource ID. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.KeyVault/vaults/{vaultName}. " +
        "Example: --source-vault /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv-amlfs-001")]
    public string? SourceVault { get; set; }

    [Option(Description = "User-assigned managed identity resource ID (full resource ID) to use for Key Vault access when custom encryption is enabled. The identity must have RBAC role to access the encryption key. " +
        "Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{name}. " +
        "Example: --user-assigned-identity-id /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity1")]
    public string? UserAssignedIdentityId { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
