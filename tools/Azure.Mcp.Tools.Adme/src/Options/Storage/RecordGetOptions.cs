// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Storage;

/// <summary>
/// Specifies the record to retrieve.
/// </summary>
public sealed class RecordGetOptions
{
    [Option(Description = "The fully-qualified record id '{partition}:{object-type}:{unique-id}', for example 'opendes:well:W-99'. Pass it verbatim as returned by the record list operation.")]
    public required string Id { get; set; }

    [Option(Description = "The numeric record version to retrieve, for example 1704779151123456. Omit to get the latest version; use the record version list operation to discover valid versions.")]
    public long? Version { get; set; }

    [Option(Description = "Dotted-path fields to return from the requested record instead of the whole record, for example 'data.WellID' and 'data.Name'. Omit to return the full record.")]
    public string[]? Attributes { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }

    [Option(Description = "The ADME resource application ID or App ID URI used as the token audience. Omit to use the standard Azure Energy resource.")]
    public string? AuthAppId { get; set; }
}
