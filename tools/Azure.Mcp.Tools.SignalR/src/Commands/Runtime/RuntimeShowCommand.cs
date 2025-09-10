// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.SignalR.Options;
using Azure.Mcp.Tools.SignalR.Options.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Runtime;

/// <summary>
/// Shows details of an Azure SignalR Service.
/// </summary>
public sealed class RuntimeShowCommand(ILogger<RuntimeShowCommand> logger)
    : BaseSignalRCommand<SignalRShowOptions>
{
    private const string CommandTitle = "Show Service Details";
    private readonly ILogger<RuntimeShowCommand> _logger = logger;

    private static readonly Option<string> _signalRNameOption = SignalROptionDefinitions.SignalR;

    public override string Name => "show";

    public override string Description =>
        """
        Show details of an Azure SignalR Runtime. Returns runtime information including location, SKU,
        provisioning state, hostname, and port configuration.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_signalRNameOption);
    }

    protected override SignalRShowOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SignalR = parseResult.GetValue(_signalRNameOption);
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
            var signalRService = context.GetService<ISignalRService>();
            var runtime = await signalRService.GetRuntimeAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.SignalR!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            context.Response.Results = runtime is null
                ? null
                : ResponseResult.Create(
                    new RuntimeShowCommandResult(runtime),
                    SignalRJsonContext.Default.RuntimeShowCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred showing SignalR service");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RuntimeShowCommandResult(Azure.Mcp.Tools.SignalR.Models.Runtime Runtime);
}
