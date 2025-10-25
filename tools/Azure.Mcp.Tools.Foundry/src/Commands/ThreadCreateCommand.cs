// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Thread;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public class ThreadCreateCommand : GlobalCommand<ThreadCreateOptions>
{
    private const string CommandTitle = "Create an AI Foundry Agent Thread";
    public override string Id => "2a30ef19-fac5-4157-8a86-30591b15818a";
    public override string Name => "create";


    public override string Description =>
        """
            Creates an AI Foundry Agent Thread that holds the messages between the Agent and the user.
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
        command.Options.Add(FoundryOptionDefinitions.UserMessageOption);
    }

    protected override ThreadCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption);
        options.UserMessage = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.UserMessage);
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
            ThreadCreateResult result = await service.CreateThread(
                options.Endpoint!,
                options.UserMessage!,
                options.Tenant
            );

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryJsonContext.Default.ThreadCreateResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
