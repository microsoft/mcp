// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.AI.Projects;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Models;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public sealed class DeploymentsListCommand : GlobalCommand<DeploymentsListOptions>
{
    private const string CommandTitle = "List Deployments from Azure AI Services";

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves a list of deployments from Azure AI Services.

        This function is used when a user requests information about the available deployments in Azure AI Services. It provides an overview of the models and services that are currently deployed and available for use.

        Usage:
            Use this function when a user wants to explore the available deployments in Azure AI Services. This can help users understand what models and services are currently operational and how they can be utilized.

        Notes:
            - The deployments listed may include various models and services that are part of Azure AI Services.
            - The list may change frequently as new deployments are added or existing ones are updated.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FoundryOptionDefinitions.EndpointOption);
    }

    protected override DeploymentsListOptions BindOptions(ParseResult parseResult)
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
            var deployments = await service.ListDeployments(
                options.Endpoint!,
                options.Tenant,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(deployments ?? []), FoundryJsonContext.Default.DeploymentsListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record DeploymentsListCommandResult(IEnumerable<Deployment> Deployments);
}
