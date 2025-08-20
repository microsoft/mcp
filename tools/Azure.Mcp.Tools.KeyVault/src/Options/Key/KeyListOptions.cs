// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.KeyVault.Options.Key
{
    public class KeyListOptions : BaseKeyVaultOptions
    {
        public bool IncludeManagedKeys { get; set; } = false;
    }
}
