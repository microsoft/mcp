// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Commands;
using Azure.Mcp.Tools.FileShares.Models;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionUpdateCommand(ILogger<PrivateEndpointConnectionUpdateCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<PrivateEndpointConnectionUpdateOptions>(logger, service)
{
    private const string CommandTitle = "Update Private Endpoint Connection";

    public override string Id => "c6d7e8f9-a0b1-4c2d-3e4f-5a6b7c8d9e0f";
    public override string Name => "update";
    public override string Description => "Update the state of a private endpoint connection for a file share. Use this to approve or reject private endpoint connection requests.";
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.FileShareName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.ConnectionName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Status.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.Description.AsOptional());
    }

    protected override PrivateEndpointConnectionUpdateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.FileShareName.Name);
        options.ConnectionName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.ConnectionName.Name);
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
            _logger.LogInformation(
                "Updating private endpoint connection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShare: {FileShareName}, Connection: {ConnectionName}, Status: {Status}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.ConnectionName, options.Status);

            var connection = await _fileSharesService.UpdatePrivateEndpointConnectionAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.ConnectionName!,
                options.Status!,
                options.Description,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new PrivateEndpointConnectionUpdateCommandResult(connection);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.PrivateEndpointConnectionUpdateCommandResult);

            _logger.LogInformation(
                "Successfully updated private endpoint connection. Connection: {ConnectionName}, Status: {Status}",
                options.ConnectionName, options.Status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update private endpoint connection");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionUpdateCommandResult([property: JsonPropertyName("connection")] PrivateEndpointConnectionInfo Connection);
}
