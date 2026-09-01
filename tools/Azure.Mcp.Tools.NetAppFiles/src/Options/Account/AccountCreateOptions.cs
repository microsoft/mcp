// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

// find: \[JsonPropertyName\(NetAppFilesOptionDefinitions.(.*)Name\)\]
// [Option(Description = NetAppFilesOptionDefinitions.$1)]

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public class AccountCreateOptions : ISubscriptionOption
{
    [Option(Description = "The name of the Azure NetApp Files account to create (e.g., 'myanfaccount').")]
    public required string Account { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Location)]
    public string? Location { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Tags)]
    public string? Tags { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KeyName)]
    public string? KeyName { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KeySource)]
    public string? KeySource { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KeyVaultResourceId)]
    public string? KeyVaultResourceId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.KeyVaultUri)]
    public string? KeyVaultUri { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.FederatedClientId)]
    public string? FederatedClientId { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.UserAssignedIdentity)]
    public string? UserAssignedIdentity { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.IdentityType)]
    public string? IdentityType { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.UserAssignedIdentities)]
    public string? UserAssignedIdentities { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ActiveDirectories)]
    public string? ActiveDirectories { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.NfsV4IdDomain)]
    public string? NfsV4IdDomain { get; set; }

    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [OptionContainer(Prefix = "retry")]
    public RetryPolicyOptions? RetryPolicy { get; set; }
}
