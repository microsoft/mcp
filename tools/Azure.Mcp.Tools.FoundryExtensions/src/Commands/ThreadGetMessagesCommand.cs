// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Thread;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class ThreadGetMessagesCommand(IFoundryExtensionsService foundryExtensionsService) : GlobalCommand<ThreadGetMessagesOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "Get Microsoft Foundry Thread Messages";

    public override string Id => "d0e1f2a3-0123-ef01-3456-789012345678";

    public override string Name => "get-messages";

    public override string Description =>
        """
        Retrieves all messages from a Microsoft Foundry AI Agent thread. Provide the project endpoint
        and thread ID to get the complete conversation history including both user messages and
        assistant responses.
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
        command.Options.Add(FoundryExtensionsOptionDefinitions.EndpointOption.AsRequired());
        command.Options.Add(FoundryExtensionsOptionDefinitions.ThreadIdOption.AsRequired());
    }

    protected override ThreadGetMessagesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.EndpointOption.Name);
        options.ThreadId = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.ThreadIdOption.Name);
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
            var result = await _foundryExtensionsService.GetMessages(
                options.Endpoint!,
                options.ThreadId!,
                options.Tenant,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryExtensionsJsonContext.Default.ThreadGetMessagesResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
