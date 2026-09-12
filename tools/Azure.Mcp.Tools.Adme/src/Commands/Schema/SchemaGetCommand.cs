// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.Adme.Options.Schema;
using Azure.Mcp.Tools.Adme.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Adme.Commands.Schema;

/// <summary>
/// Gets the JSON definition for an ADME/OSDU schema kind.
/// </summary>
[CommandMetadata(
    Id = "19fd0c96-dd65-4b8a-bfa7-4ef40b79d27a",
    Name = "get",
    Title = "Get ADME/OSDU Schema",
    Description = """
        Get one ADME/OSDU schema by exact kind.
        Returns full JSON definition, fields, property types, and structure.
        """,
    OperationPlane = ToolOperationPlane.Data,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class SchemaGetCommand(ISchemaService schemaService)
    : AuthenticatedCommand<SchemaGetOptions, JsonElement>
{
    private readonly ISchemaService _schemaService = schemaService;

    public override void ValidateOptions(SchemaGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        AdmeServiceValidator.ValidateTarget(options.Endpoint, options.DataPartition, validationResult);
        AdmeServiceValidator.ValidateKind(options.Kind, validationResult);
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
                options.Tenant,
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
