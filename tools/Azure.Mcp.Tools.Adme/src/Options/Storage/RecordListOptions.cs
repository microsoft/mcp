// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Storage;

/// <summary>
/// Specifies the kind and paging for listing OSDU record ids.
/// </summary>
public sealed class RecordListOptions
{
    [Option(Description = "The fully-qualified OSDU kind '{authority}:{source}:{entityType}:{version}', for example 'osdu:wks:master-data--Well:1.0.0'. Use 'azmcp adme schema list' to discover valid kinds.")]
    public required string Kind { get; set; }

    [Option(Description = "The number of record ids to return in one page, from 1 through 100. Defaults to 10.")]
    public int? Limit { get; set; }

    [Option(Description = "The cursor returned by a previous page. Omit for the first page; a null cursor in the response means there are no more pages.")]
    public string? Cursor { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

    [Option(Description = OptionDescriptions.Tenant)]
    public string? Tenant { get; set; }
}
