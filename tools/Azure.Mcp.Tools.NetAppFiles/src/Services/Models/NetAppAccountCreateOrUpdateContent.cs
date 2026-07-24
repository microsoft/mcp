// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.NetAppFiles.Services.Models;

/// <summary>
/// Content model for creating or updating a NetApp account via ARM generic resource API.
/// </summary>
internal sealed class NetAppAccountCreateOrUpdateContent
{
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [JsonPropertyName("tags")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string>? Tags { get; set; }

    [JsonPropertyName("properties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NetAppAccountCreateProperties? Properties { get; set; }

    [JsonPropertyName("identity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NetAppAccountCreateIdentity? Identity { get; set; }
}

internal sealed class NetAppAccountCreateProperties
{
    [JsonPropertyName("activeDirectories")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? ActiveDirectories { get; set; }

    [JsonPropertyName("nfsV4IDDomain")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NfsV4IdDomain { get; set; }

    [JsonPropertyName("encryption")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NetAppAccountCreateEncryption? Encryption { get; set; }
}

internal sealed class NetAppAccountCreateIdentity
{
    [JsonPropertyName("type")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Type { get; set; }

    [JsonPropertyName("userAssignedIdentities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public JsonElement? UserAssignedIdentities { get; set; }
}

internal sealed class NetAppAccountCreateEncryption
{
    [JsonPropertyName("keySource")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeySource { get; set; }

    [JsonPropertyName("keyVaultProperties")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NetAppAccountCreateKeyVaultProperties? KeyVaultProperties { get; set; }

    [JsonPropertyName("identity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public NetAppAccountCreateEncryptionIdentity? Identity { get; set; }
}

internal sealed class NetAppAccountCreateKeyVaultProperties
{
    [JsonPropertyName("keyName")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeyName { get; set; }

    [JsonPropertyName("keyVaultResourceId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeyVaultResourceId { get; set; }

    [JsonPropertyName("keyVaultUri")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KeyVaultUri { get; set; }
}

internal sealed class NetAppAccountCreateEncryptionIdentity
{
    [JsonPropertyName("federatedClientId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? FederatedClientId { get; set; }

    [JsonPropertyName("userAssignedIdentity")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserAssignedIdentity { get; set; }
}
