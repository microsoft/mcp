// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureManagedLustre.Options;

public static class AzureManagedLustreOptionDefinitions
{
    public const string sku = "sku";
    public const string size = "size";
    public const string location = "location";
    public const string fileSystem = "file-system";
    public const string importPrefixes = "import-prefixes";
    public const string conflictResolutionMode = "conflict-resolution-mode";
    public const string maximumErrors = "maximum-errors";
    public const string adminStatus = "admin-status";
    public const string name = "name";
    public static readonly Option<string> SkuOption = new(
        $"--{sku}"
    )
    {
        Description = "The AMLFS SKU. Allowed values: AMLFS-Durable-Premium-40, AMLFS-Durable-Premium-125, AMLFS-Durable-Premium-250, AMLFS-Durable-Premium-500.",
        Required = true
    };

    public static readonly Option<int> SizeOption = new(
        $"--{size}"
    )
    {
        Description = "The AMLFS size (TiB).",
        Required = true
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

    public static readonly Option<string> FileSystemOption = new(
        $"--{fileSystem}"
    )
    {
        Description = "The name of the Azure Managed Lustre file system (AMLFS).",
        Required = true
    };

    public static readonly Option<string[]> ImportPrefixesOption = new(
        $"--{importPrefixes}"
    )
    {
        Description = "List of path prefixes in the linked HSM/Blob container to import. Provide multiple prefixes separated by spaces. If omitted, the entire container may be considered depending on service defaults.",
        Required = false,
        AllowMultipleArgumentsPerToken = true
    };

    public static readonly Option<string> ConflictResolutionModeOption = new(
        $"--{conflictResolutionMode}"
    )
    {
        Description = "How to handle conflicts during import. Allowed values: OverwriteIfDirty, OverwriteAlways, Fail, Skip. Default: Skip.",
        Required = false
    };

    public static readonly Option<int?> MaximumErrorsOption = new(
        $"--{maximumErrors}"
    )
    {
        Description = "Maximum number of errors before the import job fails fast. Default: 0 (fail on first error).",
        Required = false
    };

    public static readonly Option<string> AdminStatusOption = new(
        $"--{adminStatus}"
    )
    {
        Description = "Administrative status of the job. Usually 'Active'.",
        Required = false
    };

    public static readonly Option<string> NameOption = new(
        $"--{name}"
    )
    {
        Description = "An optional name for the import job. If omitted a timestamp-based name will be generated.",
        Required = false
    };
}
