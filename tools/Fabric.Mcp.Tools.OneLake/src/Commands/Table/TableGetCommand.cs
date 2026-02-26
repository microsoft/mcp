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

public sealed class TableGetCommand(
    ILogger<TableGetCommand> logger,
    IOneLakeService oneLakeService) : GlobalCommand<TableGetOptions>()
{
    private readonly ILogger<TableGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "19bb5a6a-2a09-410c-bfa0-312986c6acc6";
    public override string Name => "get_table";
    public override string Title => "Get OneLake Table";
    public override string Description => "Retrieves table definition from OneLake. Use this when the user needs table schema or metadata.";

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
        command.Options.Add(FabricOptionDefinitions.Namespace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Schema.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Table.AsRequired());
    }

    protected override TableGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkspaceId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkspaceId.Name);
        options.Workspace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Workspace.Name);
        options.ItemId = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.ItemId.Name);
        options.Item = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Item.Name);
        options.Namespace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Namespace.Name);
        options.Namespace ??= parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Schema.Name);
        options.Table = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Table.Name);
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

            if (string.IsNullOrWhiteSpace(options.Namespace))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Namespace is required. Provide --namespace or --schema.";
                return context.Response;
            }

            if (string.IsNullOrWhiteSpace(options.Table))
            {
                context.Response.Status = System.Net.HttpStatusCode.BadRequest;
                context.Response.Message = "Table name is required. Provide --table.";
                return context.Response;
            }

            var tableResult = await _oneLakeService.GetTableAsync(workspaceIdentifier!, itemIdentifier!, options.Namespace!, options.Table!, cancellationToken);
            var result = new TableGetCommandResult(tableResult.Workspace, tableResult.Item, tableResult.Namespace, tableResult.Table, tableResult.Definition, tableResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table. Options: {@Options}", options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class TableGetCommandResult
    {
        public string Workspace { get; init; } = string.Empty;
        public string Item { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public string Table { get; init; } = string.Empty;
        public JsonElement Definition { get; init; } = default;
        public string RawResponse { get; init; } = string.Empty;

        public TableGetCommandResult()
        {
        }

        public TableGetCommandResult(string workspace, string item, string namespaceName, string tableName, JsonElement definition, string rawResponse)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Namespace = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
            Table = tableName ?? throw new ArgumentNullException(nameof(tableName));
            Definition = definition;
            RawResponse = rawResponse ?? string.Empty;
        }
    }
}
