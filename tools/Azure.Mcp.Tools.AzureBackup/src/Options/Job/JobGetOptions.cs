// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Job;

public sealed class JobGetOptions : BaseAzureBackupOptions
{
    [Option(Description = "The backup job ID (GUID) or the full backup job resource ID returned by job listings. When omitted, all jobs in the vault are listed.")]
    public string? Job { get; set; }
}
