// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Models;

// Deliberately excludes backend messages, which can contain credentials or resource metadata.
public sealed record BackupOperationError(string? Code, string Message);
