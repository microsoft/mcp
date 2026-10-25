// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Options.Ingestion;

public sealed class IngestionValidateOptions : ISubscriptionOption
{
    [Option(Description = OptionDescriptions.ResourceGroup)]
    public required string ResourceGroup { get; set; }

    [Option(Description = "The name of the data collection rule whose stream schema the log data is validated against.")]
    public required string DataCollectionRule { get; set; }

    [Option(Description = "The stream declared in the data collection rule to validate against, for example 'Custom-MyTable_CL'. Optional when the rule declares exactly one stream.")]
    public string? Stream { get; set; }

    [Option(Description = "The log records to validate as a JSON array of objects, up to 1 MB. For example: [{\"TimeGenerated\":\"2024-01-01T00:00:00Z\",\"Message\":\"hello\"}].")]
    public required string LogData { get; set; }

    [Option(Description = OptionDescriptions.Subscription)]
    public string? Subscription { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
