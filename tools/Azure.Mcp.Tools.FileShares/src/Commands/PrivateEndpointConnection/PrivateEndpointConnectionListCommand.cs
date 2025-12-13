// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionListCommand(ILogger<PrivateEndpointConnectionListCommand> logger)
    : BaseFileSharesCommand<PrivateEndpointConnectionListOptions>
{
    private const string CommandTitle = "List Private Endpoint Connections";
    private readonly ILogger<PrivateEndpointConnectionListCommand> _logger = logger;

    public override string Id => "a1b2c3d4-e5f6-47a8-b9c0-d1e2f3a4b5c6";

    public override string Name => "list";

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

    public override string Description =>
        "List all private endpoint connections for a file share resource.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    protected override PrivateEndpointConnectionListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
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
            var fileSharesService = context.GetService<IFileSharesService>();
            var connections = await fileSharesService.ListPrivateEndpointConnections(
                options.Subscription!,
                options.ResourceGroup!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(connections ?? []),
                FileSharesJsonContext.Default.PrivateEndpointConnectionListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing private endpoint connections.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionListCommandResult(List<PrivateEndpointConnectionData> Connections);
}
