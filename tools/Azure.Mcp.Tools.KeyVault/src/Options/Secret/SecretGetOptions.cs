// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options.Secret;

public class SecretGetOptions : BaseKeyVaultOptions
{
    public string? SecretName { get; set; }
}
