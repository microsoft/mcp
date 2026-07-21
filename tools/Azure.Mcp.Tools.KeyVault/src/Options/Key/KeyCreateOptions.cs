// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options.Key;

public sealed class KeyCreateOptions : BaseKeyVaultOptions
{
    [Option(Description = KeyVaultOptionDescriptions.Key)]
    public required string Key { get; set; }

    [Option(Description = "The type of key to create. Valid values: RSA, RSA-HSM, EC, EC-HSM. Note: RSA-HSM and EC-HSM require a premium SKU vault.")]
    public required string KeyType { get; set; }
}
