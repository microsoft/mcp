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

public sealed class ThreadCreateCommand(IFoundryExtensionsService foundryExtensionsService) : GlobalCommand<ThreadCreateOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "Create Microsoft Foundry Thread";

    public override string Id => "c9d0e1f2-9012-def0-2345-678901234567";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a new thread in Microsoft Foundry using the Azure AI Agents service. A thread represents
        a conversation with an AI agent. Provide the project endpoint and an initial user message.
        Returns the thread ID which can be used for follow-up interactions.
        """;

    public override string Title => CommandTitle;

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
        command.Options.Add(FoundryExtensionsOptionDefinitions.EndpointOption.AsRequired());
        command.Options.Add(FoundryExtensionsOptionDefinitions.UserMessageOption.AsRequired());
    }

    protected override ThreadCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.EndpointOption.Name);
        options.UserMessage = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.UserMessageOption.Name);
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
            var result = await _foundryExtensionsService.CreateThread(
                options.Endpoint!,
                options.UserMessage!,
                options.Tenant,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                result,
                FoundryExtensionsJsonContext.Default.ThreadCreateResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }
}
