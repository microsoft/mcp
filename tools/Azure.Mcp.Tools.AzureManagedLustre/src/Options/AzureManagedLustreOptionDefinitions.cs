// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
        Description = "The AMLFS SKU. Exact allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500.",
        Required = true
    };

    public static readonly Option<int> SizeOption = new(
        $"--{size}"
    )
    {
        Description = "The AMLFS size in TiB as an integer (no unit). Examples: 4, 12, 128.",
        Required = true
    };
}
