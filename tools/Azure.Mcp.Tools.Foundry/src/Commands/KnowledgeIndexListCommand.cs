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
    private const string CommandTitle = "List Knowledge Indexes in Microsoft Foundry";

    public override string Id => "346847c5-61b7-4f14-b484-e28ee3e7c0f0";

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves a list of knowledge indexes from Microsoft Foundry.

        This function is used when a user requests information about the available knowledge indexes in Microsoft Foundry. It provides an overview of the knowledge bases and search indexes that are currently deployed and available for use with AI agents and applications.
        Requires the project endpoint URL (format: https://<resource>.services.ai.azure.com/api/projects/<project-name>).
        Usage:
            Use this function when a user wants to explore the available knowledge indexes in Microsoft Foundry. This can help users understand what knowledge bases are currently operational and how they can be utilized for retrieval-augmented generation (RAG) scenarios.
        
        Notes:
            - The indexes listed are knowledge indexes specifically created within Microsoft Foundry projects.
            - These indexes can be used with AI agents for knowledge retrieval and RAG applications.
            - The list may change as new indexes are created or existing ones are updated.
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
        command.Options.Add(FoundryOptionDefinitions.EndpointOption);
    }

    protected override KnowledgeIndexListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption.Name);

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
            var service = context.GetService<IFoundryService>();
            var indexes = await service.ListKnowledgeIndexes(
                options.Endpoint!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(new(indexes ?? []), FoundryJsonContext.Default.KnowledgeIndexListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record KnowledgeIndexListCommandResult(IEnumerable<KnowledgeIndexInformation> Indexes);
}
