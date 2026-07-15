// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.OneLake.Commands.Table;

[CommandMetadata(
    Id = "19bb5a6a-2a09-410c-bfa0-312986c6acc6",
    Name = "get_table",
    Title = "Get OneLake Table",
    Description = "Retrieves table definition from OneLake. Use this when the user needs table schema or metadata.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class TableGetCommand(
    ILogger<TableGetCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<TableGetOptions, TableGetCommand.TableGetCommandResult>
{
    private readonly ILogger<TableGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(TableGetOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (string.IsNullOrWhiteSpace(options.WorkspaceId) && string.IsNullOrWhiteSpace(options.Workspace))
        {
            validationResult.Errors.Add("Workspace identifier is required. Provide --workspace or --workspace-id.");
        }

        if (string.IsNullOrWhiteSpace(options.ItemId) && string.IsNullOrWhiteSpace(options.Item))
        {
            validationResult.Errors.Add("Item identifier is required. Provide --item or --item-id.");
        }

        if (string.IsNullOrWhiteSpace(options.Namespace) && string.IsNullOrWhiteSpace(options.Schema))
        {
            validationResult.Errors.Add("Namespace is required. Provide --namespace or --schema.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableGetOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
        var itemId = !string.IsNullOrWhiteSpace(options.ItemId) ? options.ItemId : options.Item;
        var ns = !string.IsNullOrWhiteSpace(options.Namespace) ? options.Namespace : options.Schema!;

        try
        {
            var tableResult = await _oneLakeService.GetTableAsync(workspaceId!, itemId!, ns, options.Table!, cancellationToken);
            var result = new TableGetCommandResult(tableResult.Workspace, tableResult.Item, tableResult.Namespace, tableResult.Table, tableResult.Definition, tableResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}, Table: {Table}.", workspaceId, itemId, options.Table);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TableGetCommandResult(
        string Workspace,
        string Item,
        string Namespace,
        string Table,
        JsonElement Definition,
        string RawResponse);
}
