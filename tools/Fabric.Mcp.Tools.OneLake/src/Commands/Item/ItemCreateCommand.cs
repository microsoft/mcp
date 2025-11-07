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

public sealed class ItemCreateCommand(
    ILogger<ItemCreateCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<ItemCreateOptions>()
{
    private readonly ILogger<ItemCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Name => "item-create";
    public override string Title => "Create Fabric Item";
    public override string Description => "Create a new item (Lakehouse, Notebook, etc.) in a Microsoft Fabric workspace using the Fabric API.";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        LocalRequired = false,
        OpenWorld = false,
        ReadOnly = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.DisplayName.AsRequired());
        command.Options.Add(FabricOptionDefinitions.ItemType.AsRequired());
        command.Options.Add(FabricOptionDefinitions.Description.AsOptional());
    }

    protected override ItemCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        var workspaceName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.ItemName = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.DisplayName.Name) ?? string.Empty;
        options.ItemType = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemType.Name) ?? string.Empty;
        options.ItemDescription = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Description.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        try
        {
            if (string.IsNullOrWhiteSpace(options.WorkspaceId))
            {
                throw new ArgumentException("Workspace identifier is required. Provide --workspace or --workspace-id.", nameof(options.WorkspaceId));
            }

            var request = new CreateItemRequest
            {
                DisplayName = options.ItemName,
                Type = options.ItemType,
                Description = options.ItemDescription
            };

            var item = await _oneLakeService.CreateItemAsync(options.WorkspaceId, request, CancellationToken.None);

            _logger.LogInformation("Successfully created {ItemType} '{ItemName}' in workspace {WorkspaceId}",
                options.ItemType, options.ItemName, options.WorkspaceId);

            var result = new ItemCreateCommandResult(item);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.ItemCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item '{ItemName}' in workspace {WorkspaceId}. Options: {@Options}",
                options.ItemName, options.WorkspaceId, options);
            HandleException(context, ex);
        }
        
        return context.Response;
    }

    public sealed record ItemCreateCommandResult(OneLakeItem Item);
}

public sealed class ItemCreateOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
}