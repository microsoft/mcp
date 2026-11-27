// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Ingestion;

/// <summary>
/// A single problem found while validating log data for ingestion.
/// </summary>
/// <param name="RecordIndex">The zero-based index of the offending record, or null when the issue applies to the whole payload.</param>
/// <param name="Column">The column the issue applies to, if any.</param>
/// <param name="Message">A description of the issue.</param>
public sealed record IngestionValidationIssue(int? RecordIndex, string? Column, string Message);
