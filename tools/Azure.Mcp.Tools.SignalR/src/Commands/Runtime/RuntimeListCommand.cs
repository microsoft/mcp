// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.SignalR.Options.Runtime;
using Azure.Mcp.Tools.SignalR.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.SignalR.Commands.Runtime;

/// <summary>
/// Lists Azure SignalR Service resources in the specified subscription.
/// </summary>
public sealed class RuntimeListCommand(ILogger<RuntimeListCommand> logger)
    : SubscriptionCommand<SignalRListOptions>
{
    private const string CommandTitle = "List all Runtimes";
    private readonly ILogger<RuntimeListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List all SignalR Runtime resources in a specified subscription.
        Returns an array of SignalR Runtime details.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var signalRService = context.GetService<ISignalRService>() ??
                                 throw new InvalidOperationException("SignalR service is not available.");
            var runtimes = await signalRService.ListRuntimesAsync(
                options.Subscription!,
                options.Tenant,
                options.AuthMethod,
                options.RetryPolicy);

            _logger.LogInformation("Found {Count} SignalR service(s) in subscription {SubscriptionId}",
                runtimes.Count(), options.Subscription);

            context.Response.Results = runtimes.Any()
                ? ResponseResult.Create(
                    new RuntimeListCommandResult(runtimes),
                    SignalRJsonContext.Default.RuntimeListCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing SignalR services in subscription {SubscriptionId}",
                options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record RuntimeListCommandResult(IEnumerable<Azure.Mcp.Tools.SignalR.Models.Runtime> Runtimes);
}
