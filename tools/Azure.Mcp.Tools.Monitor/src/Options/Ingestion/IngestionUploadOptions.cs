// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Ingestion;

public sealed class IngestionUploadOptions
{
    [Option(Description = "The data collection endpoint (DCE) log ingestion URL, for example 'https://my-dce.eastus-1.ingest.monitor.azure.com'.")]
    public required string IngestionEndpoint { get; set; }

    [Option(Description = "The immutable ID of the data collection rule (for example 'dcr-0123456789abcdef0123456789abcdef').")]
    public required string DataCollectionRule { get; set; }

    [Option(Description = "The stream declared in the data collection rule that receives the data, for example 'Custom-MyTable_CL'.")]
    public required string Stream { get; set; }

    [Option(Description = "The log records to upload as a JSON array of objects, up to 1 MB. For example: [{\"TimeGenerated\":\"2024-01-01T00:00:00Z\",\"Message\":\"hello\"}].")]
    public required string LogData { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
