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
    Id = "7b1688e5-2a16-475d-8fd1-9bf3b0acf4f7",
    Name = "list_tables",
    Title = "List OneLake Tables",
    Description = "Lists tables in OneLake. Use this when the user needs to see available tables.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class TableListCommand(ILogger<TableListCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<TableListOptions, TableListCommand.TableListCommandResult>
{
    private readonly ILogger<TableListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(TableListOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableListOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
        var itemId = !string.IsNullOrWhiteSpace(options.ItemId) ? options.ItemId : options.Item;
        var ns = !string.IsNullOrWhiteSpace(options.Namespace) ? options.Namespace : options.Schema!;

        try
        {
            var tablesResult = await _oneLakeService.ListTablesAsync(workspaceId!, itemId!, ns, cancellationToken);
            var result = new TableListCommandResult(tablesResult.Workspace, tablesResult.Item, tablesResult.Namespace, tablesResult.Tables, tablesResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}, Namespace: {Namespace}.", workspaceId, itemId, ns);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TableListCommandResult(
        string Workspace,
        string Item,
        string Namespace,
        JsonElement Tables,
        string RawResponse);
}
