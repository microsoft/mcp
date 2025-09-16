// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureManagedLustre.Options;

public static class AzureManagedLustreOptionDefinitions
{
    public const string sku = "sku";
    public const string size = "size";
    public const string location = "location";
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
}
