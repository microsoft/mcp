// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionUpdateCommand(ILogger<PrivateEndpointConnectionUpdateCommand> logger)
    : BaseFileSharesCommand<PrivateEndpointConnectionUpdateOptions>
{
    private const string CommandTitle = "Update Private Endpoint Connection";
    private readonly ILogger<PrivateEndpointConnectionUpdateCommand> _logger = logger;

    public override string Id => "c3d4e5f6-a7b8-49c0-d1e2-f3a4b5c6d7e8";

    public override string Name => "update";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        "Update the approval status of a private endpoint connection.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.Account.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Name);
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Status.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Description.AsOptional());
    }

    protected override PrivateEndpointConnectionUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Account = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.Account.Name);
        options.Name = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.Name.Name);
        options.Status = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.Status.Name);
        options.Description = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.Description.Name);
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
            var connection = await fileSharesService.UpdatePrivateEndpointConnection(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Status!,
                options.Description,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(connection),
                FileSharesJsonContext.Default.PrivateEndpointConnectionUpdateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred updating private endpoint connection.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionUpdateCommandResult(PrivateEndpointConnectionData? Connection);
}
