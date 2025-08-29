// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.Identity;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Identity;

/// <summary>
/// Shows the managed identity configuration of an Azure SignalR Service.
/// </summary>
public sealed class IdentityShowCommand(ILogger<IdentityShowCommand> logger)
    : BaseSignalRCommand<IdentityShowOptions>
{
    private const string CommandTitle = "Show Identity Configuration";
    private readonly ILogger<IdentityShowCommand> _logger = logger;

    private static readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalRName;

    public override string Name => "show";

    public override string Description =>
        """
        Show the managed identity configuration of an Azure SignalR Service. Returns identity information
        including type (SystemAssigned, UserAssigned), principal ID, tenant ID, and any
        user-assigned identities associated with the service.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.AddOption(_signalRNameOption);
    }

    protected override IdentityShowOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SignalRName = parseResult.GetValueForOption(_signalRNameOption);
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

            var signalRService = context.GetService<ISignalRService>();
            var identity = await signalRService.GetSignalRIdentityAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalRName!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = identity is null
                ? null
                : ResponseResult.Create(
                    new IdentityShowCommandResult(identity),
                    SignalRJsonContext.Default.IdentityShowCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred showing SignalR identity");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record IdentityShowCommandResult(Models.Identity Identity);
}
