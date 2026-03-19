// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class KnowledgeIndexSchemaCommand(IFoundryExtensionsService foundryExtensionsService) : GlobalCommand<KnowledgeIndexSchemaOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "Get Knowledge Index Schema in Microsoft Foundry";

    public override string Id => "c3d4e5f6-3456-789a-cdef-012345678901";

    public override string Name => "schema";

    public override string Description =>
        """
        Retrieves the detailed schema configuration of a specific knowledge index from Microsoft Foundry.

        This function provides comprehensive information about the structure and configuration of a knowledge index, including field definitions, data types, searchable attributes, and other schema properties. The schema information is essential for understanding how the index is structured and how data is indexed and searchable.

        Usage:
            Use this function when you need to examine the detailed configuration of a specific knowledge index. This is helpful for troubleshooting search issues, understanding index capabilities, planning data mapping, or when integrating with the index programmatically.

        Notes:
            - Returns the index schema.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FoundryExtensionsOptionDefinitions.EndpointOption);
        command.Options.Add(FoundryExtensionsOptionDefinitions.IndexNameOption);
    }

    protected override KnowledgeIndexSchemaOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.EndpointOption.Name);
        options.IndexName = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.IndexNameOption.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var indexSchema = await _foundryExtensionsService.GetKnowledgeIndexSchema(
                options.Endpoint!,
                options.IndexName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            if (indexSchema == null)
            {
                throw new Exception("Failed to retrieve knowledge index schema - no data returned.");
            }

            context.Response.Results = ResponseResult.Create(new(indexSchema), FoundryExtensionsJsonContext.Default.KnowledgeIndexSchemaCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KnowledgeIndexSchemaCommandResult(KnowledgeIndexSchema Schema);
}
