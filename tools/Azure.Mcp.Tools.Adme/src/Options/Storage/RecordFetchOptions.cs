// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Storage;

/// <summary>
/// Specifies the OSDU records to fetch in one call and how to shape them.
/// </summary>
public sealed class RecordFetchOptions
{
    [Option(Description = "The fully-qualified OSDU record ids to fetch, each '{partition}:{group-type}--{EntityType}:{unique-id}'. Up to 20 ids without --attributes, or 100 with it.")]
    public required string[] Ids { get; set; }

    [Option(Description = "Dotted-path fields to return instead of whole records, for example 'data.Name' and 'data.FileSourceInfo'. Use it only when specific fields were requested; it cannot be combined with --frame-of-reference.")]
    public string[]? Attributes { get; set; }

    [Option(Description = "Convert measurements to SI, coordinates to WGS84 and dates to UTC on the server. The response then carries conversionStatuses, which is empty for records without measured or spatial fields.")]
    public bool FrameOfReference { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
