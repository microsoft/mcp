// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options.Secret;

public sealed class SecretCreateOptions : BaseKeyVaultOptions
{
    [Option(Description = KeyVaultOptionDescriptions.Secret)]
    public required string Secret { get; set; }

    [Option(Description = "The value to set for the secret.")]
    public required string Value { get; set; }
}
