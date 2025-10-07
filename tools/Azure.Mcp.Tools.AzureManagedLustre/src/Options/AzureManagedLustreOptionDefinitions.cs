// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureManagedLustre.Options;

public static class AzureManagedLustreOptionDefinitions
{
    public const string sku = "sku";
    public const string size = "size";
    public const string name = "name";
    public const string location = "location";
    public const string subnetId = "subnet-id";
    public const string zone = "zone";
    public const string hsmContainer = "hsm-container";
    public const string hsmLogContainer = "hsm-log-container";
    public const string importPrefix = "import-prefix";
    public const string maintenanceDay = "maintenance-day";
    public const string maintenanceTime = "maintenance-time";
    public const string rootSquashMode = "root-squash-mode";
    public const string noSquashNidLists = "no-squash-nid-list";
    public const string squashUid = "squash-uid";
    public const string squashGid = "squash-gid";
    public const string customEncryption = "custom-encryption";
    public const string keyUrl = "key-url";
    public const string sourceVault = "source-vault";
    public const string userAssignedIdentityId = "user-assigned-identity-id";

    public static readonly Option<string> SkuOption = new(
        $"--{sku}"
    )
    {
        Required = true,
        Description = "The AMLFS SKU. Exact allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500."
    };

    public static readonly Option<int> SizeOption = new(
        $"--{size}"
    )
    {
        Required = true,
        Description = "The AMLFS size in TiB as an integer (no unit). Examples: 4, 12, 128."
    };

    public static readonly Option<string> LocationOption = new(
        $"--{location}"
    )
    {
        Description = "Azure region/region short name (use Azure location token, lowercase). Examples: uaenorth, swedencentral, eastus.",
        Required = true
    };

    public static readonly Option<string> OptionalLocationOption = new(
        $"--{location}"
    )
    {
        Description = LocationOption.Description,
        Required = false
    };

    public static readonly Option<string> NameOption = new(
        $"--{name}"
    )
    {
        Required = true,
        Description = "The AMLFS resource name. Must be DNS-friendly (letters, numbers, hyphens). Example: --name amlfs-001"
    };

    public static readonly Option<string> SubnetIdOption = new(
        $"--{subnetId}"
    )
    {
        Required = true,
        Description = "Full subnet resource ID. Required format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}.\n" +
                      "Example: --subnet-id /subscriptions/0000/resourceGroups/my-rg/providers/Microsoft.Network/virtualNetworks/vnet-001/subnets/subnet-001"
    };

    public static readonly Option<string> ZoneOption = new(
        $"--{zone}"
    )
    {
        Required = true,
        Description = "Availability zone identifier. Use a single digit string matching the region's AZ labels (e.g. '1').\n" +
                      "Example: --zone 1"
    };

    public static readonly Option<string> HsmContainerOption = new(
        $"--{hsmContainer}"
    )
    {
        Required = false,
        Description = "Full blob container resource ID for HSM integration. HPC Cache Resource Provider must have before deployment Storage Blob Data Contributor and Storage Account Contributor roles on parent Storage Account." +
                      "Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Storage/storageAccounts/{account}/blobServices/default/containers/{container}.\n" +
                      "Example: --hsm-container /subscriptions/0000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stacc/blobServices/default/containers/hsm-container\n"
    };

    public static readonly Option<string> HsmLogContainerOption = new(
        $"--{hsmLogContainer}"
    )
    {
        Required = false,
        Description = "Full blob container resource ID for HSM logging. HPC Cache Resource Provider must have before deployment Storage Blob Data Contributor and Storage Account Contributor roles on parent Storage Account. Same format as --hsm-container.\n" +
                      "Example: --hsm-log-container /subscriptions/0000/resourceGroups/rg/providers/Microsoft.Storage/storageAccounts/stacc/blobServices/default/containers/hsm-logs\n"
    };

    public static readonly Option<string> ImportPrefixOption = new(
        $"--{importPrefix}"
    )
    {
        Required = false,
        Description = "Optional HSM import prefix (path prefix inside the container starting with /). Examples: '/ingest/', '/archive/2019/'.\n"
    };

    public static readonly Option<string> MaintenanceDayOption = new(
        $"--{maintenanceDay}"
    )
    {
        Required = true,
        Description = "Preferred maintenance day. Allowed values: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday.\n"
    };

    public static readonly Option<string> MaintenanceTimeOption = new(
        $"--{maintenanceTime}"
    )
    {
        Required = true,
        Description = "Preferred maintenance time in UTC. Format: HH:MM (24-hour). Examples: 00:00, 23:00.\n"
    };

    public static readonly Option<string> RootSquashModeOption = new(
        $"--{rootSquashMode}"
    )
    {
        Required = false,
        Description = "Root squash mode. Allowed values: All, RootOnly, None.\n"
    };

    public static readonly Option<string> NoSquashNidListsOption = new(
        $"--{noSquashNidLists}"
    )
    {
        Required = false,
        Description = "Comma-separated list of NIDs (network identifiers) not to squash. Example: '10.0.2.4@tcp;10.0.2.[6-8]@tcp'.\n"
    };

    public static readonly Option<long?> SquashUidOption = new(
        $"--{squashUid}"
    )
    {
        Required = false,
        Description = "Numeric UID to squash root to. Required in case root squash mode is not None. Example: --squash-uid 1000.\n"
    };

    public static readonly Option<long?> SquashGidOption = new(
        $"--{squashGid}"
    )
    {
        Required = false,
        Description = "Numeric GID to squash root to.  Required in case root squash mode is not None. Example: --squash-gid 1000.\n"
    };

    public static readonly Option<bool> CustomEncryptionOption = new(
        $"--{customEncryption}"
    )
    {
        Required = false,
        Description = "Enable customer-managed encryption using a Key Vault key. When true, --key-url and --source-vault required, with a user-assigned identity already configured for Key Vault key access."
    };

    public static readonly Option<string> KeyUrlOption = new(
        $"--{keyUrl}"
    )
    {
        Required = false,
        Description = "Full Key Vault key URL. Format: https://{vaultName}.vault.azure.net/keys/{keyName}/{keyVersion}.\n" +
                      "Example: --key-url https://kv-amlfs-001.vault.azure.net/keys/key-amlfs-001/0000\n"
    };

    public static readonly Option<string> SourceVaultOption = new(
        $"--{sourceVault}"
    )
    {
        Required = false,
        Description = "Full Key Vault resource ID. Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.KeyVault/vaults/{vaultName}.\n" +
                      "Example: --source-vault /subscriptions/0000/resourceGroups/rg/providers/Microsoft.KeyVault/vaults/kv-amlfs-001\n"
    };

    public static readonly Option<string> UserAssignedIdentityIdOption = new(
        $"--{userAssignedIdentityId}"
    )
    {
        Required = false,
        Description = "User-assigned managed identity resource ID (full resource ID) to use for Key Vault access when custom encryption is enabled. The identity must have RBAC role to access the encryption key\n" +
                      "Format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{name}.\n" +
                      "Example: --user-assigned-identity-id /subscriptions/0000/resourceGroups/rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/identity1\n"
    };

    public static readonly Option<string> OptionalMaintenanceDayOption = MaintenanceDayOption.AsOptional();
    public static readonly Option<string> OptionalMaintenanceTimeOption = MaintenanceTimeOption.AsOptional();
}

