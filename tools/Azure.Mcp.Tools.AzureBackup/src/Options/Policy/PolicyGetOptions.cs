// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public class PolicyGetOptions : BaseAzureBackupOptions
{
    [Option(AzureBackupOptionDefinitions.Policy)]
    public string? Policy { get; set; }
}
