// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureBackup.Options.Policy;

public sealed class PolicyGetOptions : BaseAzureBackupOptions
{
    [Option(Description = AzureBackupOptionDefinitions.Policy)]
    public string? Policy { get; set; }
}
