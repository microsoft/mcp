// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Security;

public sealed class SecurityConfigureMuaOptions : BaseAzureBackupOptions
{
    [Option(Description = "ARM resource ID of the Resource Guard to link for Multi-User Authorization (e.g., '/subscriptions/.../resourceGroups/.../providers/Microsoft.DataProtection/resourceGuards/myGuard').")]
    public string? ResourceGuardId { get; set; }
}
