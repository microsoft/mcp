// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
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

    public override string Id => "b4e0e0e1-e2e3-e4e5-e6e7-e8e9eaebecea";
    public override string Name => "update";
    public override string Description => "Update the approval status of a private endpoint connection for an Azure managed file share. Note: Updates are typically done through the private endpoint resource.";
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
        options.ConnectionState = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.Status.Name);
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
            _logger.LogInformation("Updating private endpoint connection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, ConnectionName: {ConnectionName}, Status: {Status}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.ConnectionName, options.ConnectionState);

            var connection = await _fileSharesService.UpdatePrivateEndpointConnectionAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.ConnectionName!,
                options.ConnectionState!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new PrivateEndpointConnectionUpdateCommandResult(connection);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.PrivateEndpointConnectionUpdateCommandResult);

            _logger.LogInformation("Private endpoint connection updated successfully");
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

