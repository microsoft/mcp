// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Blob.Container;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Blob.Container;

public sealed class ContainerGetCommand(ILogger<ContainerGetCommand> logger) : BaseStorageCommand<ContainerGetOptions>()
{
    private const string CommandTitle = "Get Storage Container Details";
    private readonly ILogger<ContainerGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        $"""
        Gets the details of Azure Storage containers, including metadata, lease status, access level, and more. If a specific
        container name is not provided, the command will return details for all containers within the specified account.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Container.AsOptional());
    }

    protected override ContainerGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Container = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Container.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var storageService = context.GetService<IStorageService>();
            var containers = await storageService.GetContainerDetails(
                options.Account!,
                options.Container,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy
            );

            context.Response.Results = containers is { Count: > 0 }
                ? ResponseResult.Create(new ContainerGetCommandResult(containers), StorageJsonContext.Default.ContainerGetCommandResult)
                : null;
            return context.Response;
        }
        catch (Exception ex)
        {
            if (options.Container is null)
            {
                _logger.LogError(ex, "Error listing container details. Account: {Account}.", options.Account);
            }
            else
            {
                _logger.LogError(ex, "Error getting container details. Account: {Account}, Container: {Container}.", options.Account, options.Container);
            }
            HandleException(context, ex);
            return context.Response;
        }
    }

    internal record ContainerGetCommandResult([property: JsonPropertyName("containers")] List<ContainerInfo> Containers);
}
