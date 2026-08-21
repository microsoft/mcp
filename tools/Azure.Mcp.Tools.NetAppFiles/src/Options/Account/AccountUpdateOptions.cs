// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Options.Account;

public class AccountUpdateOptions : BaseNetAppFilesOptions
{
    [JsonPropertyName(NetAppFilesOptionDefinitions.IdsName)]
    public string[]? Ids { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.LocationName)]
    public string? Location { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.TagsName)]
    public string? Tags { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KeyNameName)]
    public string? KeyName { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KeySourceName)]
    public string? KeySource { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KeyVaultResourceIdName)]
    public string? KeyVaultResourceId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.KeyVaultUriName)]
    public string? KeyVaultUri { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.FederatedClientIdName)]
    public string? FederatedClientId { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UserAssignedIdentityName)]
    public string? UserAssignedIdentity { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.IdentityTypeName)]
    public string? IdentityType { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.UserAssignedIdentitiesName)]
    public string? UserAssignedIdentities { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ActiveDirectoriesName)]
    public string? ActiveDirectories { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NfsV4IdDomainName)]
    public string? NfsV4IdDomain { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.NoWaitName)]
    public bool NoWait { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.AddName)]
    public string[]? Add { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.SetName)]
    public string[]? Set { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.RemoveName)]
    public string[]? Remove { get; set; }

    [JsonPropertyName(NetAppFilesOptionDefinitions.ForceStringName)]
    public bool ForceString { get; set; }
}
