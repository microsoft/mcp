// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

// note: can DRY this further by combining AccountGetOptions and AccountUpdateOptions common options into a single class
public class AccountUpdateOptions : BaseNetAppFilesOptions
{
    [Option(Description = NetAppFilesOptionDefinitions.Ids)]
    public string[]? Ids { get; set; }

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

    [Option(Description = NetAppFilesOptionDefinitions.NoWait)]
    public bool NoWait { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Add)]
    public string[]? Add { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Set)]
    public string[]? Set { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.Remove)]
    public string[]? Remove { get; set; }

    [Option(Description = NetAppFilesOptionDefinitions.ForceString)]
    public bool ForceString { get; set; }
}
