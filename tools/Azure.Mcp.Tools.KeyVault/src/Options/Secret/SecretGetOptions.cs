// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options.Secret;

public sealed class SecretGetOptions : BaseKeyVaultOptions
{
    [Option(Description = KeyVaultOptionDescriptions.Secret)]
    public string? Secret { get; set; }
}
