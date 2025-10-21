// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class KnowledgeIndexListCommand : GlobalCommand<KnowledgeIndexListOptions>
{
    private const string CommandTitle = "List Knowledge Indexes in Azure AI Foundry";

    public override string Name => "list";

    public override string Description =>
        """
        List knowledge indexes from an Azure AI Foundry project. Shows available knowledge bases and search indexes 
        for retrieval-augmented generation (RAG) scenarios with AI agents. Requires the project endpoint URL 
        (format: https://<resource>.services.ai.azure.com/api/projects/<project-name>).
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
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
                options.RetryPolicy);

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
