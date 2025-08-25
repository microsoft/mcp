// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class KnowledgeIndexSchemaCommand : GlobalCommand<KnowledgeIndexSchemaOptions>
{
    private const string CommandTitle = "Get Knowledge Index Schema in Azure AI Foundry";
    private readonly Option<string> _endpointOption = FoundryOptionDefinitions.EndpointOption;
    private readonly Option<string> _indexNameOption = FoundryOptionDefinitions.IndexNameOption;

    public override string Name => "schema";

    public override string Description =>
        """
        Retrieves the detailed schema configuration of a specific knowledge index from Azure AI Foundry.

        This function provides comprehensive information about the structure and configuration of a knowledge index, including field definitions, data types, searchable attributes, and other schema properties. The schema information is essential for understanding how the index is structured and how data is indexed and searchable.

        Usage:
            Use this function when you need to examine the detailed configuration of a specific knowledge index. This is helpful for troubleshooting search issues, understanding index capabilities, planning data mapping, or when integrating with the index programmatically.

        Notes:
            - Returns the complete index schema including field definitions, analyzers, and configuration settings.
            - The schema provides insights into searchable, filterable, and retrievable field configurations.
            - Index schema information is useful for query planning and data integration tasks.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_endpointOption);
        command.AddOption(_indexNameOption);
    }

    protected override KnowledgeIndexSchemaOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueForOption(_endpointOption);
        options.IndexName = parseResult.GetValueForOption(_indexNameOption);

        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var service = context.GetService<IFoundryService>();
            var indexSchema = await service.GetKnowledgeIndexSchema(
                options.Endpoint!,
                options.IndexName!,
                options.Tenant,
                options.RetryPolicy);

            if (indexSchema == null)
            {
                throw new Exception("Failed to retrieve knowledge index schema - no data returned.");
            }

            context.Response.Results = ResponseResult.Create(
                new KnowledgeIndexSchemaCommandResult(indexSchema),
                FoundryJsonContext.Default.KnowledgeIndexSchemaCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KnowledgeIndexSchemaCommandResult(object Schema);
}
