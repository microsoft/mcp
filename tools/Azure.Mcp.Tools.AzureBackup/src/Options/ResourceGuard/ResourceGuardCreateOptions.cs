// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.ResourceGuard;

public sealed class ResourceGuardCreateOptions : ISubscriptionOption
{
    [Option(Description = "Name of the Resource Guard to create (Microsoft.DataProtection/resourceGuards).")]
    public required string ResourceGuard { get; set; }

    [Option(Description = "Azure region where the Resource Guard is created. Must match the region of vaults that will link to it (e.g., 'eastus2', 'westeurope').")]
    public required string Location { get; set; }

    [Option(Description = "Comma-separated list of critical operations to EXCLUDE from Resource Guard protection. Valid RSV values: deleteProtection, getSecurityPIN, updatePolicy, updateProtection. Omit to protect all default critical operations.")]
    public string? ExcludedOperations { get; set; }

    [Option(Description = "Tags to apply to the Resource Guard, in 'key1=value1,key2=value2' format.")]
    public string? Tags { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }
}
