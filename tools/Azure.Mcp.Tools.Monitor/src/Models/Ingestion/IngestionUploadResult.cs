// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Ingestion;

/// <summary>
/// The outcome of uploading log data through the Azure Monitor Logs Ingestion API.
/// </summary>
/// <param name="Status">The ingestion operation status.</param>
/// <param name="DataCollectionRule">The data collection rule immutable ID used.</param>
/// <param name="Stream">The stream the records were sent to.</param>
/// <param name="RecordCount">The number of records uploaded.</param>
public sealed record IngestionUploadResult(string Status, string DataCollectionRule, string Stream, int RecordCount);
