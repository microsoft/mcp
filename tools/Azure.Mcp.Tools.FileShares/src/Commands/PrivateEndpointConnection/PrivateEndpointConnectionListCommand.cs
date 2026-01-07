// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

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
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionListCommand(ILogger<PrivateEndpointConnectionListCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<PrivateEndpointConnectionListOptions>(logger, service)
{
    private const string CommandTitle = "List Private Endpoint Connections";

    public override string Id => "b2e0e0e1-e2e3-e4e5-e6e7-e8e9eaebecea";
    public override string Name => "list";
    public override string Description => "List all private endpoint connections for an Azure managed file share. Private endpoints enable secure, private access to file shares.";
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
    }

    protected override PrivateEndpointConnectionListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileShareName = parseResult.GetValueOrDefault<string>(FileSharesOptionDefinitions.PrivateEndpointConnection.FileShareName.Name);
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
            _logger.LogInformation("Listing private endpoint connections. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}",
                options.Subscription, options.ResourceGroup, options.FileShareName);

            var connections = await _fileSharesService.ListPrivateEndpointConnectionsAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new PrivateEndpointConnectionListCommandResult(connections ?? []);
            context.Response.Results = ResponseResult.Create(result, FileSharesJsonContext.Default.PrivateEndpointConnectionListCommandResult);

            _logger.LogInformation("Found {Count} private endpoint connections", connections?.Count ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list private endpoint connections");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionListCommandResult([property: JsonPropertyName("connections")] List<PrivateEndpointConnectionInfo> Connections);
}

