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
    Id = "a86298d1-7475-4ea8-8c1b-e4c54ac2b896",
    Name = "get_table_namespace",
    Title = "Get OneLake Table Namespace",
    Description = "Retrieves metadata for a specific table namespace. Use this when the user needs details about a namespace.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class TableNamespaceGetCommand(ILogger<TableNamespaceGetCommand> logger, IOneLakeService oneLakeService)
    : AuthenticatedCommand<TableNamespaceGetOptions, TableNamespaceGetCommand.TableNamespaceGetCommandResult>
{
    private readonly ILogger<TableNamespaceGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(TableNamespaceGetOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableNamespaceGetOptions options, CancellationToken cancellationToken)
    {
        var workspaceId = !string.IsNullOrWhiteSpace(options.WorkspaceId) ? options.WorkspaceId : options.Workspace;
        var itemId = !string.IsNullOrWhiteSpace(options.ItemId) ? options.ItemId : options.Item;
        var ns = !string.IsNullOrWhiteSpace(options.Namespace) ? options.Namespace : options.Schema!;

        try
        {
            var namespaceResult = await _oneLakeService.GetTableNamespaceAsync(workspaceId!, itemId!, ns, cancellationToken);
            var result = new TableNamespaceGetCommandResult(namespaceResult.Workspace, namespaceResult.Item, namespaceResult.Namespace, namespaceResult.Definition, namespaceResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableNamespaceGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table namespace. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}, Namespace: {Namespace}.", workspaceId, itemId, ns);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record TableNamespaceGetCommandResult(
        string Workspace,
        string Item,
        string Namespace,
        JsonElement Definition,
        string RawResponse);
}
