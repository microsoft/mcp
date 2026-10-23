// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Security.KeyVault.Keys;

namespace Azure.Mcp.Tools.KeyVault.Models;

public sealed class KeyDetails
{
    public required string Name { get; init; }
    public required string KeyType { get; init; }
    public bool? Enabled { get; init; }
    public DateTimeOffset? NotBefore { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? UpdatedOn { get; init; }

    public static KeyDetails FromKey(KeyVaultKey key) => new()
    {
        Name = key.Name,
        KeyType = key.KeyType.ToString(),
        Enabled = key.Properties.Enabled,
        NotBefore = key.Properties.NotBefore,
        ExpiresOn = key.Properties.ExpiresOn,
        CreatedOn = key.Properties.CreatedOn,
        UpdatedOn = key.Properties.UpdatedOn
    };
}
