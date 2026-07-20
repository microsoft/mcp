// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.ManagedLustre.Options;

internal static class ManagedLustreOptionDescriptions
{
    internal const string Sku = "The AMLFS SKU. Exact allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500.";
    internal const string Size = "The AMLFS size in TiB as an integer (no unit). Examples: 4, 12, 128.";
    internal const string Location = "Azure region/region short name (use Azure location token, lowercase). Examples: uaenorth, swedencentral, eastus.";
    internal const string Name = "The AMLFS resource name. Must be DNS-friendly (letters, numbers, hyphens). Example: --name amlfs-001";
    internal const string SubnetId = "Full subnet resource ID. Required format: /subscriptions/{sub}/resourceGroups/{rg}/providers/Microsoft.Network/virtualNetworks/{vnet}/subnets/{subnet}. " +
        "Example: --subnet-id /subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/my-rg/providers/Microsoft.Network/virtualNetworks/vnet-001/subnets/subnet-001";
    internal const string MaintenanceDay = "Preferred maintenance day. Allowed values: Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday.";
    internal const string MaintenanceTime = "Preferred maintenance time in UTC. Format: HH:MM (24-hour). Examples: 00:00, 23:00.";
    internal const string RootSquashMode = "Root squash mode. Allowed values: All, RootOnly, None.";
    internal const string NoSquashNidLists = "Comma-separated list of NIDs (network identifiers) not to squash. Example: '10.0.2.4@tcp;10.0.2.[6-8]@tcp'.";
    internal const string SquashUid = "Numeric UID to squash root to. Required in case root squash mode is not None. Example: --squash-uid 1000.";
    internal const string SquashGid = "Numeric GID to squash root to.  Required in case root squash mode is not None. Example: --squash-gid 1000.";
    internal const string FileSystemName = "The name of the Azure Managed Lustre filesystem";
    internal const string AutoexportJobName = "The name of the autoexport job";
    internal const string AutoimportJobName = "The name of the autoimport job";
    internal const string ImportJobName = "The name of the import job";
    internal const string AdminStatus = "The administrative status of the auto import job. Enable: job is active. Disable: disables the current active auto import job. Default: Enable. Allowed values: Enable, Disable.";
}
