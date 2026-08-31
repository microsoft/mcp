// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Adme.Options.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Schema;

/// <summary>
/// Gets the JSON definition for an ADME schema kind.
/// </summary>
[CommandMetadata(
    Id = "19fd0c96-dd65-4b8a-bfa7-4ef40b79d27a",
    Name = "get",
    Title = "Get ADME Schema",
    Description = """
        Get the full JSON schema definition for one OSDU kind - its fields, types, and structure - from a data
        partition.

        Required: --kind, --endpoint, and --data-partition.

        --kind must be a FULLY-QUALIFIED kind 'authority:source:type:version', for example
        'osdu:wks:master-data--Well:1.0.0'. Wildcards are not supported; use 'azmcp adme schema list' to
        discover which kinds and versions exist before calling this tool.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class SchemaGetCommand(ISchemaService schemaService)
    : BaseCommand<SchemaGetOptions, JsonElement>
{
    private readonly ISchemaService _schemaService = schemaService;

    public override void ValidateOptions(SchemaGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceHelper.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
    }

    /// <summary>
    /// Executes the schema retrieval request.
    /// </summary>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context, SchemaGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _schemaService.GetSchemaAsync(
                options.Endpoint,
                options.DataPartition,
                options.Kind,
                cancellationToken);
            context.Response.Results = ResponseResult.Create(result, AdmeJsonContext.Default.JsonElement);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
