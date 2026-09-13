// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Storage;

/// <summary>
/// Specifies the OSDU record to retrieve.
/// </summary>
public sealed class RecordGetOptions
{
    [Option(Description = "The fully-qualified OSDU record id '{partition}:{group-type}--{EntityType}:{unique-id}', for example 'opendes:master-data--Well:W-99'. Pass it verbatim as returned by 'azmcp adme storage record list'.")]
    public required string Id { get; set; }

    [Option(Description = "The numeric record version to retrieve, for example 1704779151123456. Omit to get the latest version; use 'azmcp adme storage record version list' to discover valid versions.")]
    public long? Version { get; set; }

    [Option(Description = "Dotted-path fields to return from the requested record instead of the whole record, for example 'data.WellID' and 'data.Name'. Omit to return the full record.")]
    public string[]? Attributes { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
