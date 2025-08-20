// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options.Secret;

public class SecretCreateOptions : BaseKeyVaultOptions
{
    public string? SecretName { get; set; }
    public string? SecretValue { get; set; }
}
