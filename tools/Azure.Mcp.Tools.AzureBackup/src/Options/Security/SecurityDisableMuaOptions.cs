// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Security;

public sealed class SecurityDisableMuaOptions : BaseAzureBackupOptions
{
    [Option(Description = "Required confirmation flag. Set --force to acknowledge that disabling Multi-User Authorization removes the Resource Guard's protection from critical vault operations (disable soft delete, remove immutability, stop protection). Without this flag the command fails.")]
    public bool Force { get; set; }
}
