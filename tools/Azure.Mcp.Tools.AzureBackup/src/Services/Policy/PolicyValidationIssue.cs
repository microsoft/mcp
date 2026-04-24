// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBackup.Services.Policy;

/// <summary>
/// A single problem detected by <see cref="PolicyCreateValidator"/>.
/// <see cref="Flag"/> identifies the offending command-line option (e.g. "--schedule-frequency");
/// or "policy" when the issue spans multiple flags.
/// </summary>
public sealed record PolicyValidationIssue(string Flag, string Message);
