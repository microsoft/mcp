// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.Core.Models;
using Fabric.Mcp.Tools.Core.Options;
using Fabric.Mcp.Tools.Core.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.Core.Commands;

public sealed class ItemCreateCommand(
    ILogger<ItemCreateCommand> logger,
    IFabricCoreService fabricCoreService) : GlobalCommand<ItemCreateOptions>()
{
    private readonly ILogger<ItemCreateCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricCoreService _fabricCoreService = fabricCoreService ?? throw new ArgumentNullException(nameof(fabricCoreService));

    public override string Id => "bfdfd3c0-4551-4454-a930-5bf5b1ad5690";
    public override string Name => "create_item";
    public override string Title => "Create Fabric Item";
    public override string Description => "Creates a new item in a Fabric workspace. Use this when the user wants to create a Lakehouse, Notebook, or other Fabric item type. Requires workspace ID, item name, and item type.";

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
        command.Options.Add(CoreOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(CoreOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(CoreOptionDefinitions.DisplayName.AsRequired());
        command.Options.Add(CoreOptionDefinitions.ItemType.AsRequired());
        command.Options.Add(CoreOptionDefinitions.Description.AsOptional());
    }

    protected override ItemCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        var workspaceId = parseResult.GetValueOrDefault<string>(CoreOptionDefinitions.WorkspaceIdName);
        var workspaceName = parseResult.GetValueOrDefault<string>(CoreOptionDefinitions.WorkspaceName);
        options.WorkspaceId = !string.IsNullOrWhiteSpace(workspaceId)
            ? workspaceId!
            : workspaceName ?? string.Empty;
        options.ItemName = parseResult.GetValueOrDefault<string>(CoreOptionDefinitions.DisplayNameName) ?? string.Empty;
        options.ItemType = parseResult.GetValueOrDefault<string>(CoreOptionDefinitions.ItemTypeName) ?? string.Empty;
        options.ItemDescription = parseResult.GetValueOrDefault<string>(CoreOptionDefinitions.DescriptionName);
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

            var item = await _fabricCoreService.CreateItemAsync(options.WorkspaceId, request, cancellationToken);

            _logger.LogInformation("Successfully created {ItemType} '{ItemName}' in workspace {WorkspaceId}",
                options.ItemType, options.ItemName, options.WorkspaceId);

            var result = new ItemCreateCommandResult(item);
            context.Response.Results = ResponseResult.Create(result, CoreJsonContext.Default.ItemCreateCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating item '{ItemName}' in workspace {WorkspaceId}. Options: {@Options}",
                options.ItemName, options.WorkspaceId, options);
            HandleException(context, ex);
        }

        return context.Response;
    }
}

public sealed class ItemCreateOptions : GlobalOptions
{
    public string WorkspaceId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
}
