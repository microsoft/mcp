// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Adme.Models.Schema;
using Azure.Mcp.Tools.Adme.Options.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Schema;

/// <summary>
/// Lists ADME/OSDU schema descriptors matching the requested filters.
/// </summary>
[CommandMetadata(
    Id = "456a50dc-cdfa-49d8-8f9f-7e8b063898f5",
    Name = "list",
    Title = "List ADME/OSDU Schemas",
    Description = """
        List multiple ADME/OSDU schema descriptors.

        Optional parameters: authority, source, entity type, status (PUBLISHED, DEVELOPMENT, OBSOLETE),
        scope (SHARED, INTERNAL), schema version (major, minor, patch), latest version, offset, and limit.
        """,
    OperationPlane = ToolOperationPlane.Data,
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
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);

        if (options.LatestVersion && options.SchemaVersionMinor.HasValue && !options.SchemaVersionMajor.HasValue)
        {
            validationResult.Errors.Add("--schema-version-minor requires --schema-version-major when --latest-version is true.");
        }

        if (options.LatestVersion && options.SchemaVersionPatch.HasValue && !options.SchemaVersionMinor.HasValue)
        {
            validationResult.Errors.Add("--schema-version-patch requires --schema-version-minor when --latest-version is true.");
        }

        if (options.Offset < 0)
        {
            validationResult.Errors.Add("--offset must not be negative.");
        }

        if (options.Limit < 0)
        {
            validationResult.Errors.Add("--limit must not be negative.");
        }
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
                options.Limit,
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
