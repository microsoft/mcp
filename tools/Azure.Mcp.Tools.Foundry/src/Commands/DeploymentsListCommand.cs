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
        Lists Azure AI Foundry (Cognitive Services) model deployments at a given account endpoint and shows currently provisioned model deployments. Use to audit what is deployed before invoking or creating new deployments. Do not use this tool to discover undeployed catalog/base models — instead, use models_list tool.
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
