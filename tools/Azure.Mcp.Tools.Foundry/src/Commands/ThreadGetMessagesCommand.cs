// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Foundry.Models;
using Azure.Mcp.Tools.Foundry.Options;
using Azure.Mcp.Tools.Foundry.Options.Thread;
using Azure.Mcp.Tools.Foundry.Services;

namespace Azure.Mcp.Tools.Foundry.Commands;

public class ThreadGetMessagesCommand : GlobalCommand<ThreadGetMessagesOptions>
{
    private const string CommandTitle = "Get messages in an AI Foundry Agent Thread";
    public override string Id => "7769d80f-31b0-4bc3-9e34-991e23d00fee";
    public override string Name => "get-messages";


    public override string Description =>
        """
            Get messages in an AI Foundry Agent Thread.
        """;

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
        command.Options.Add(FoundryOptionDefinitions.ThreadIdOption);
    }

    protected override ThreadGetMessagesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.EndpointOption);
        options.ThreadId = parseResult.GetValueOrDefault<string>(FoundryOptionDefinitions.ThreadIdOption);
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
            ThreadGetMessagesResult result = await service.GetMessages(
                options.Endpoint!,
                options.ThreadId!,
                options.Tenant
            );

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryJsonContext.Default.ThreadGetMessagesResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
