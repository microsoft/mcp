// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services;

/// <summary>
/// A backup job reference parsed from user input.
/// </summary>
/// <param name="JobName">The job name (typically a GUID) to pass to the SDK.</param>
/// <param name="VaultType">The vault type inferred from a full ARM job resource ID ("rsv" or "dpp"), or null when not inferable.</param>
internal sealed record BackupJobReference(string JobName, string? VaultType);
