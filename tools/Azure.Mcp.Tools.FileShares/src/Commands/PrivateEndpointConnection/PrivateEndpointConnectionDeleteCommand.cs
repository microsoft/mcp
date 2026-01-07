// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Options.PrivateEndpointConnection;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.FileShares.Commands.PrivateEndpointConnection;

public sealed class PrivateEndpointConnectionDeleteCommand(ILogger<PrivateEndpointConnectionDeleteCommand> logger, IFileSharesService service)
    : BaseFileSharesCommand<PrivateEndpointConnectionDeleteOptions>(logger, service)
{
    private const string CommandTitle = "Delete Private Endpoint Connection";

    public override string Id => "b5e0e0e1-e2e3-e4e5-e6e7-e8e9eaebecea";
    public override string Name => "delete";
    public override string Description => "Delete a private endpoint connection from an Azure managed file share. Note: Deletion is typically done through the private endpoint resource itself.";
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
    }

    protected override PrivateEndpointConnectionDeleteOptions BindOptions(ParseResult parseResult)
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
            _logger.LogInformation("Deleting private endpoint connection. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, FileShareName: {FileShareName}, ConnectionName: {ConnectionName}",
                options.Subscription, options.ResourceGroup, options.FileShareName, options.ConnectionName);

            await _fileSharesService.DeletePrivateEndpointConnectionAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileShareName!,
                options.ConnectionName!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            var result = new PrivateEndpointConnectionDeleteCommandResult(
                true,
                options.ConnectionName!,
                options.FileShareName!,
                options.ResourceGroup!);
            context.Response.Results = ResponseResult.Create(
                result,
                FileSharesJsonContext.Default.PrivateEndpointConnectionDeleteCommandResult);

            _logger.LogInformation("Private endpoint connection deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete private endpoint connection");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record PrivateEndpointConnectionDeleteCommandResult(
        [property: JsonPropertyName("deleted")] bool Deleted,
        [property: JsonPropertyName("connectionName")] string ConnectionName,
        [property: JsonPropertyName("fileShareName")] string FileShareName,
        [property: JsonPropertyName("resourceGroup")] string ResourceGroup);
}

