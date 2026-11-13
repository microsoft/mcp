// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Ingestion;

/// <summary>
/// The outcome of validating log data against Azure Monitor ingestion requirements and a data collection rule schema.
/// </summary>
/// <param name="IsValid">True when no errors were found.</param>
/// <param name="DataCollectionRule">The data collection rule used for schema validation, if any.</param>
/// <param name="Stream">The stream whose declared schema was used, if any.</param>
/// <param name="RecordCount">The number of records found in the payload.</param>
/// <param name="SchemaChecked">True when records were compared against a declared stream schema.</param>
/// <param name="Errors">Problems that will cause ingestion to fail or data to be rejected.</param>
/// <param name="Warnings">Problems that will not fail ingestion but may drop or null out data.</param>
public sealed record IngestionValidationResult(
    bool IsValid,
    string? DataCollectionRule,
    string? Stream,
    int RecordCount,
    bool SchemaChecked,
    IReadOnlyList<IngestionValidationIssue> Errors,
    IReadOnlyList<IngestionValidationIssue> Warnings);
