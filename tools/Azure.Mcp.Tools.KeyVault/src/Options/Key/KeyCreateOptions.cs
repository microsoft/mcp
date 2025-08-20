// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options.Key;

public class KeyCreateOptions : BaseKeyVaultOptions
{
    public string? KeyName { get; set; }
    public string? KeyType { get; set; }
}
