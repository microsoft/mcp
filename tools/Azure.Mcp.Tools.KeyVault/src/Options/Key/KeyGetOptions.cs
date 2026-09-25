// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options.Key;

public sealed class KeyGetOptions : BaseKeyVaultOptions
{
    [Option(Description = KeyVaultOptionDescriptions.Key)]
    public string? Key { get; set; }

    [Option(Description = "Whether or not to include managed keys in results.")]
    public bool IncludeManaged { get; set; }
}
