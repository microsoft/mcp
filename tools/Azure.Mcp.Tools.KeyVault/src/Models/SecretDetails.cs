// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Security.KeyVault.Secrets;

namespace Azure.Mcp.Tools.KeyVault.Models;

public sealed record SecretDetails
{
    public required string Name { get; init; }
    public required string Value { get; init; }
    public bool? Enabled { get; init; }
    public DateTimeOffset? NotBefore { get; init; }
    public DateTimeOffset? ExpiresOn { get; init; }
    public DateTimeOffset? CreatedOn { get; init; }
    public DateTimeOffset? UpdatedOn { get; init; }

    public static SecretDetails FromSecret(KeyVaultSecret secret) => new()
    {
        Name = secret.Name,
        Value = secret.Value,
        Enabled = secret.Properties.Enabled,
        NotBefore = secret.Properties.NotBefore,
        ExpiresOn = secret.Properties.ExpiresOn,
        CreatedOn = secret.Properties.CreatedOn,
        UpdatedOn = secret.Properties.UpdatedOn
    };
}
