// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

[CommandMetadata(
    Id = "c3d4e5f6-3456-789a-cdef-012345678901",
    Name = "schema",
    Title = "Get Knowledge Index Schema in Microsoft Foundry",
    Description = """
        Retrieves the detailed schema configuration of a specific knowledge index from Microsoft Foundry.

        This function provides comprehensive information about the structure and configuration of a knowledge index, including field definitions, data types, searchable attributes, and other schema properties. The schema information is essential for understanding how the index is structured and how data is indexed and searchable.

        Usage:
            Use this function when you need to examine the detailed configuration of a specific knowledge index. This is helpful for troubleshooting search issues, understanding index capabilities, planning data mapping, or when integrating with the index programmatically.

        Notes:
            - Returns the index schema.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class KnowledgeIndexSchemaCommand(IFoundryExtensionsService foundryExtensionsService)
    : AuthenticatedCommand<KnowledgeIndexSchemaOptions, KnowledgeIndexSchemaCommand.KnowledgeIndexSchemaCommandResult>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    public override void ValidateOptions(KnowledgeIndexSchemaOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        FoundryExtensionsHelpers.ValidateFoundryEndpoint(options.Endpoint, validationResult);
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, KnowledgeIndexSchemaOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var indexSchema = await _foundryExtensionsService.GetKnowledgeIndexSchema(
                options.Endpoint,
                options.Index,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken: cancellationToken) ?? throw new Exception("Failed to retrieve knowledge index schema - no data returned.");

            context.Response.Results = ResponseResult.Create(new(indexSchema), FoundryExtensionsJsonContext.Default.KnowledgeIndexSchemaCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record KnowledgeIndexSchemaCommandResult(KnowledgeIndexSchema Schema);
}
