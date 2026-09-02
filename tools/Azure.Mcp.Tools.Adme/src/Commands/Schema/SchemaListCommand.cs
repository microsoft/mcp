// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Schema;
using Azure.Mcp.Tools.Adme.Options.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Schema;

/// <summary>
/// Lists ADME schema descriptors matching the requested filters.
/// </summary>
[CommandMetadata(
    Id = "456a50dc-cdfa-49d8-8f9f-7e8b063898f5",
    Name = "list",
    Title = "List ADME Schemas",
    Description = """
        List OSDU schemas (which kinds/versions exist) in a data partition, optionally filtered. Returns
        lightweight descriptors (id, entityType, version, status, scope) - NOT the full field definitions;
        use 'azmcp adme schema get' for those.

        Required: --endpoint and --data-partition. Optional: --tenant for cross-tenant authentication.

        Filters: --authority (e.g. 'osdu'), --source (e.g. 'wks'), --entity-type (e.g. 'master-data--Well');
        --status ('PUBLISHED', 'DEVELOPMENT', or 'OBSOLETE'); --scope ('SHARED' = system/OSDU schemas,
        'INTERNAL' = tenant/partition-defined); --schema-version-major/minor/patch; --latest-version to
        return only the newest version per entity; --offset and --limit for paging (the response carries
        offset and totalCount).

        When --latest-version is true, supply version filters in order: major, then minor, then patch.
        A minor version without a major version, or a patch version without a minor version, is invalid.

        With no status filter, results mix PUBLISHED/DEVELOPMENT/OBSOLETE. To enumerate usable kinds,
        prefer status='PUBLISHED' and surface each result's status to the user for clarity.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class SchemaListCommand(ISchemaService schemaService)
    : AuthenticatedCommand<SchemaListOptions, SchemaListResponse>
{
    private readonly ISchemaService _schemaService = schemaService;

    public override void ValidateOptions(SchemaListOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceHelper.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
    }

    /// <summary>
    /// Executes the schema listing request.
    /// </summary>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, SchemaListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _schemaService.ListSchemasAsync(
                options.Endpoint,
                options.DataPartition,
                options.Tenant,
                options.Authority,
                options.Source,
                options.EntityType,
                options.Status,
                options.Scope,
                options.SchemaVersionMajor,
                options.SchemaVersionMinor,
                options.SchemaVersionPatch,
                options.LatestVersion,
                options.Offset,
                options.Limit ?? 100,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.SchemaListResponse);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
