// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Job;

public sealed class JobGetOptions : BaseAzureBackupOptions
{
    [Option(Description = "The backup job ID.")]
    public string? Job { get; set; }
}
