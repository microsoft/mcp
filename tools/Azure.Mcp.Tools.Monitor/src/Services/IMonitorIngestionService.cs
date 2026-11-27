// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Models.Ingestion;

namespace Azure.Mcp.Tools.Monitor.Services;

/// <summary>
/// Uploads and validates custom log data for Azure Monitor ingestion using data collection rules.
/// </summary>
public interface IMonitorIngestionService
{
    /// <summary>
    /// Uploads log records to a data collection endpoint using a data collection rule.
    /// </summary>
    Task<IngestionUploadResult> UploadLogsAsync(
        string ingestionEndpoint,
        string dataCollectionRule,
        string stream,
        string logData,
        string? tenant,
        CancellationToken cancellationToken);

    /// <summary>
    /// Validates log records against payload requirements and the schema declared by a data collection rule.
    /// </summary>
    Task<IngestionValidationResult> ValidateLogDataAsync(
        string subscription,
        string resourceGroup,
        string dataCollectionRule,
        string? stream,
        string logData,
        string? tenant,
        CancellationToken cancellationToken);
}
