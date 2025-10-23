// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.AI.Agents.Persistent;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Agents;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public class AgentsCreateCommand : GlobalCommand<AgentsCreateOptions>
{
    private const string CommandTitle = "Create an AI Foundry Agent";
    public override string Name => "create";


    public override string Description =>
        """
            Creates an AI Foundry Agent that processes messages according to a given system instruction using an existing AI Foundry model deployment.
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FoundryOptionDefinitions.EndpointOption);
        command.Options.Add(FoundryOptionDefinitions.ModelDeploymentNameOption);
        command.Options.Add(FoundryOptionDefinitions.AgentNameOption);
        command.Options.Add(FoundryOptionDefinitions.SystemInstructionOption);
    }

    protected override AgentsCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption);
        options.ModelDeploymentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ModelDeploymentNameOption);
        options.AgentName = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.AgentNameOption);
        options.SystemInstruction = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.SystemInstructionOption);
        return options;
    }

    public override string Title => CommandTitle;

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
            AgentsCreateResult result = await service.CreateAgent(
                options.Endpoint!,
                options.ModelDeploymentName!,
                options.AgentName!,
                options.SystemInstruction!,
                options.Tenant
            );

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryJsonContext.Default.AgentsCreateResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}

