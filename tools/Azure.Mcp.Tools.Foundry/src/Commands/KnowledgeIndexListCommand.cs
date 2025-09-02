// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class KnowledgeIndexListCommand : GlobalCommand<KnowledgeIndexListOptions>
{
    private const string CommandTitle = "List Knowledge Indexes in Azure AI Foundry";
    private readonly Option<string> _endpointOption = FoundryOptionDefinitions.EndpointOption;

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves a list of knowledge indexes from Azure AI Foundry.

        This function is used when a user requests information about the available knowledge indexes in Azure AI Foundry. It provides an overview of the knowledge bases and search indexes that are currently deployed and available for use with AI agents and applications.

        Usage:
            Use this function when a user wants to explore the available knowledge indexes in Azure AI Foundry. This can help users understand what knowledge bases are currently operational and how they can be utilized for retrieval-augmented generation (RAG) scenarios.

        Notes:
            - The indexes listed are knowledge indexes specifically created within Azure AI Foundry projects.
            - These indexes can be used with AI agents for knowledge retrieval and RAG applications.
            - The list may change as new indexes are created or existing ones are updated.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_endpointOption);
    }

    protected override KnowledgeIndexListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueForOption(_endpointOption);

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
            var indexes = await service.ListKnowledgeIndexes(
                options.Endpoint!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = indexes?.Count > 0 ?
                ResponseResult.Create(
                    new KnowledgeIndexListCommandResult(indexes),
                    FoundryJsonContext.Default.KnowledgeIndexListCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KnowledgeIndexListCommandResult(IEnumerable<KnowledgeIndexInformation> Indexes);
}
