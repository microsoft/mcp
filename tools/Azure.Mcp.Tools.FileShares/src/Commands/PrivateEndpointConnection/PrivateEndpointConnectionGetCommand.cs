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

public sealed class PrivateEndpointConnectionGetCommand(ILogger<PrivateEndpointConnectionGetCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<PrivateEndpointConnectionGetOptions>(logger, service)
{
    private const string CommandTitle = "Get Private Endpoint Connection";

    public override string Id => "b3e0e0e1-e2e3-e4e5-e6e7-e8e9eaebecea";
    public override string Name => "get";
    public override string Description => "Get private endpoint connections for an Azure managed file share. If connection-name is provided, returns a specific connection; otherwise, lists all connections for the file share.";
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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.FileShareName.AsRequired());
        command.Options.Add(FileSharesOptionDefinitions.PrivateEndpointConnection.ConnectionName.AsOptional());
    }

    protected override PrivateEndpointConnectionGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.FileShareName.Name);
        options.ConnectionName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.ConnectionName.Name);
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
            // If connection name is not provided, list all connections
            if (string.IsNullOrEmpty(options.ConnectionName))
            {
                _logger.LogInformation("Listing private endpoint connections. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName);

                var connections = await _fileSharesService.ListPrivateEndpointConnectionsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileShareName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var listResult = new PrivateEndpointConnectionListResult(connections ?? []);
                context.Response.Results = ResponseResult.Create(listResult, FileSharesJsonContext.Default.PrivateEndpointConnectionListResult);

                _logger.LogInformation("Found {Count} private endpoint connections", connections?.Count ?? 0);
            }
            else
            {
                _logger.LogInformation("Getting private endpoint connection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, ConnectionName: {ConnectionName}",
                    options.Subscription, options.ResourceGroup, options.FileShareName, options.ConnectionName);

                var connection = await _fileSharesService.GetPrivateEndpointConnectionAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileShareName!,
                    options.ConnectionName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                var result = new PrivateEndpointConnectionGetCommandResult(connection);
                context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.PrivateEndpointConnectionGetCommandResult);

                _logger.LogInformation("Retrieved private endpoint connection successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get private endpoint connection");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionGetCommandResult([property: JsonPropertyName("connection")] PrivateEndpointConnectionInfo Connection);
    internal record PrivateEndpointConnectionListResult([property: JsonPropertyName("connections")] List<PrivateEndpointConnectionInfo> Connections);
}

