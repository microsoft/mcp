// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionGetCommand(ILogger<PrivateEndpointConnectionGetCommand> logger)
    : BaseFileSharesCommand<PrivateEndpointConnectionGetOptions>
{
    private const string CommandTitle = "Get Private Endpoint Connection";
    private readonly ILogger<PrivateEndpointConnectionGetCommand> _logger = logger;

    public override string Id => "b2c3d4e5-f6a7-48b9-c0d1-e2f3a4b5c6d7";

    public override string Name => "get";

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
        "Get details about a specific private endpoint connection.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.Account.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Name);
    }

    protected override PrivateEndpointConnectionGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Account = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Account.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.Name.Name);
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
            var connection = await fileSharesService.GetPrivateEndpointConnection(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(connection),
                FileSharesJsonContext.Default.PrivateEndpointConnectionGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred getting private endpoint connection.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionGetCommandResult(PrivateEndpointConnectionData? Connection);
}
