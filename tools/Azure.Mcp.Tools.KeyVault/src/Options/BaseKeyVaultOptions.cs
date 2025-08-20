// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.KeyVault.Options;

public class BaseKeyVaultOptions : SubscriptionOptions
{
    public string? VaultName { get; set; }
}
