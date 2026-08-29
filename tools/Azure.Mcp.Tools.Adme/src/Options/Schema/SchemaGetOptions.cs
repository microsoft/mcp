// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Schema;

/// <summary>
/// Specifies the ADME schema kind to retrieve.
/// </summary>
public sealed class SchemaGetOptions
{
    [Option(Description = "The fully-qualified OSDU kind 'authority:source:type:version', for example 'osdu:wks:master-data--Well:1.0.0'. Wildcards are not supported; use 'azmcp adme schema list' to discover valid kinds and versions.")]
    public required string Kind { get; set; }

    [Option(Description = "The Azure Data Manager for Energy endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The ADME data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

}
