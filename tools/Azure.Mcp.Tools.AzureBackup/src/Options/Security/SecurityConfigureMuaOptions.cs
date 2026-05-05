// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.AzureBackup.Options.Security;

public class SecurityConfigureMuaOptions : BaseAzureBackupOptions
{
    [JsonPropertyName(AzureBackupOptionDefinitions.ResourceGuardIdName)]
    public string? ResourceGuardId { get; set; }
}
