// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionDeleteCommand(ILogger<PrivateEndpointConnectionDeleteCommand> logger)
    : BaseFileSharesCommand<PrivateEndpointConnectionDeleteOptions>
{
    private const string CommandTitle = "Delete Private Endpoint Connection";
    private readonly ILogger<PrivateEndpointConnectionDeleteCommand> _logger = logger;

    public override string Id => "d4e5f6a7-b8c9-40d0-e1f2-a3b4c5d6e7f8";

    public override string Name => "delete";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    public override string Description =>
        "Delete a private endpoint connection.";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.Account.AsOptional());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Name);
    }

    protected override PrivateEndpointConnectionDeleteOptions BindOptions(ParseResult parseResult)
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
            await fileSharesService.DeletePrivateEndpointConnection(
                options.Subscription!,
                options.ResourceGroup!,
                options.Name!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(true), FileSharesJsonContext.Default.PrivateEndpointConnectionDeleteCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred deleting private endpoint connection.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionDeleteCommandResult(bool Success);
}
