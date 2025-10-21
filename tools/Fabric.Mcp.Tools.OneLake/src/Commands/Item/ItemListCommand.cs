// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace Fabric.Mcp.Tools.OneLake.Commands.Item;

public sealed class ItemListCommand(
    ILogger<ItemListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ItemListOptions>()
{
    private readonly ILogger<ItemListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "item-list";
    public override string Title => "List Fabric Items";
    public override string Description => "List all items in a Microsoft Fabric workspace, optionally filtered by item type.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = true,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId);
        command.Options.Add(FabricOptionDefinitions.ItemType);
        command.Options.Add(FabricOptionDefinitions.Recursive);
        command.Options.Add(FabricOptionDefinitions.ContinuationToken);
    }

    protected override ItemListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceIdName) ?? string.Empty;
        options.ItemType = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemTypeName);
        options.Recursive = parseResult.GetValueOrDefault<bool>(FabricOptionDefinitions.RecursiveName);
        options.ContinuationToken = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ContinuationTokenName);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            var items = await _oneLakeService.ListItemsAsync(
                options.WorkspaceId,
                options.ItemType,
                options.Recursive,
                null, // rootFolderId
                options.ContinuationToken,
                CancellationToken.None);

            var result = new ItemListCommandResult(items);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ItemListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing items in workspace {WorkspaceId}. Options: {@Options}", options.WorkspaceId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record ItemListCommandResult(
        IEnumerable<OneLakeItem> Items);
}

public sealed class ItemListOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string? ItemType { get; set; }
    public bool Recursive { get; set; } = true;
    public string? ContinuationToken { get; set; }
}