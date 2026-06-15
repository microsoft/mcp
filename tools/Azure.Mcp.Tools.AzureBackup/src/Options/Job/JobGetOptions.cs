// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Job;

public class JobGetOptions : BaseAzureBackupOptions
{
    [Option("The backup job ID.")]
    public string? Job { get; set; }
}
