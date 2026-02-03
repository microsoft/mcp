// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Table;

public sealed class TableNamespaceListCommand(
    ILogger<TableNamespaceListCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<TableNamespaceListOptions>()
{
    private readonly ILogger<TableNamespaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "173cfc00-7c12-486d-a0e7-c0d4c1de23fd";
    public override string Name => "list";
    public override string Title => "List OneLake Table Namespaces";
    public override string Description => "Enumerate namespaces available for a OneLake warehouse or lakehouse table endpoint using the OneLake Table API. CRITICAL: When using --item with friendly names, MUST include the item type suffix (e.g., 'ItemName.Lakehouse' or 'ItemName.Warehouse').";

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
        command.Options.Add(FabricOptionDefinitions.WorkspaceId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Workspace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.ItemId.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Item.AsOptional());
    }

    protected override TableNamespaceListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
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
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            if (string.IsNullOrWhiteSpace(workspaceIdentifier))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Workspace identifier is required. Provide --workspace or --workspace-id.";
                return context.Response;
            }

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            if (string.IsNullOrWhiteSpace(itemIdentifier))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Item identifier is required. Provide --item or --item-id.";
                return context.Response;
            }

            var namespaceResult = await _oneLakeService.ListTableNamespacesAsync(workspaceIdentifier!, itemIdentifier!, cancellationToken);
            var result = new TableNamespaceListCommandResult(namespaceResult.Workspace, namespaceResult.Item, namespaceResult.Namespaces, namespaceResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableNamespaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing table namespaces. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class TableNamespaceListCommandResult
    {
        public string Workspace { get; init; } = string.Empty;
        public string Item { get; init; } = string.Empty;
        public JsonElement Namespaces { get; init; } = default;
        public string RawResponse { get; init; } = string.Empty;

        public TableNamespaceListCommandResult()
        {
        }

        public TableNamespaceListCommandResult(string workspace, string item, JsonElement namespaces, string rawResponse)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Namespaces = namespaces;
            RawResponse = rawResponse ?? string.Empty;
        }
    }
}
