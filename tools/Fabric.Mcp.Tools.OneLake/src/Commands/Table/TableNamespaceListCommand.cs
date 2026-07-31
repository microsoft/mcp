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
    Id = "173cfc00-7c12-486d-a0e7-c0d4c1de23fd",
    Name = "list_table_namespaces",
    Title = "List OneLake Table Namespaces",
    Description = "Lists table namespaces in OneLake. Use this when the user needs to discover available table namespaces.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class TableNamespaceListCommand(ILogger<TableNamespaceListCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<TableNamespaceListOptions, TableNamespaceListCommand.TableNamespaceListCommandResult>
{
    private readonly ILogger<TableNamespaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(TableNamespaceListOptions options, ValidationResult validationResult)
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
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableNamespaceListOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
        var itemId = !string.IsNullOrWhiteSpace(options.ItemId) ? options.ItemId : options.Item;

        try
        {
            var namespaceResult = await _oneLakeService.ListTableNamespacesAsync(workspaceId!, itemId!, cancellationToken);
            var result = new TableNamespaceListCommandResult(namespaceResult.Workspace, namespaceResult.Item, namespaceResult.Namespaces, namespaceResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableNamespaceListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing table namespaces. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}.", workspaceId, itemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TableNamespaceListCommandResult(string Workspace, string Item, JsonElement Namespaces, string RawResponse);
}
