// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Storage;

/// <summary>
/// Specifies the OSDU record whose versions are listed.
/// </summary>
public sealed class RecordVersionListOptions
{
    [Option(Description = "The fully-qualified OSDU record id '{partition}:{group-type}--{EntityType}:{unique-id}', for example 'opendes:master-data--Well:W-99'.")]
    public required string Id { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
