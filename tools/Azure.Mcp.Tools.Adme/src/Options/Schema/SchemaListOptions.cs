// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Schema;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Adme.Options.Schema;

/// <summary>
/// Specifies filters and paging for listing ADME schemas.
/// </summary>
public sealed class SchemaListOptions
{
    [Option(Description = "Filter by the authority segment of the kind, for example 'osdu' in 'osdu:wks:master-data--Well:1.0.0'.")]
    public string? Authority { get; set; }

    [Option(Description = "Filter by the source segment of the kind, for example 'wks' in 'osdu:wks:master-data--Well:1.0.0'.")]
    public string? Source { get; set; }

    [Option(Description = "Filter by the entity type segment of the kind, for example 'master-data--Well' or 'work-product-component--WellLog'.")]
    public string? EntityType { get; set; }

    [Option(Description = "Filter by lifecycle status: PUBLISHED, DEVELOPMENT, or OBSOLETE. Defaults to PUBLISHED, which is what you usually want when enumerating usable kinds; widen it only when the user explicitly asks for draft or retired schemas.")]
    public SchemaStatus? Status { get; set; } = SchemaStatus.PUBLISHED;

    [Option(Description = "Filter by scope: SHARED for system-defined OSDU schemas, INTERNAL for schemas defined in this data partition. Omit to return both.")]
    public SchemaScope? Scope { get; set; }

    [Option(Description = "Filter by schema major version, for example 1. When combined with --latest-version, supply version filters in order (major, then minor).")]
    public int? SchemaVersionMajor { get; set; }

    [Option(Description = "Filter by schema minor version, for example 0. When --latest-version is true, requires --schema-version-major.")]
    public int? SchemaVersionMinor { get; set; }

    [Option(Description = "Filter by schema patch version, for example 0. When --latest-version is true, requires --schema-version-major and --schema-version-minor.")]
    public int? SchemaVersionPatch { get; set; }

    [Option(Description = "Return only the newest version of each schema entity, collapsing duplicates across scopes. When filtering by version, supply components in order: major, then minor, then patch.")]
    public bool LatestVersion { get; set; }

    [Option(Description = "The starting offset for paging. Defaults to 0; compare with the response's totalCount to decide whether to fetch further pages.")]
    public int Offset { get; set; }

    [Option(Description = "The number of schema descriptors to return in one page, from 0 through 100. Defaults to 100.")]
    public int? Limit { get; set; }

    [Option(Description = "The service endpoint, for example 'https://contoso.energy.azure.com'.")]
    public required string Endpoint { get; set; }

    [Option(Description = "The data partition to target, for example 'contoso-dp1'.")]
    public required string DataPartition { get; set; }

}
