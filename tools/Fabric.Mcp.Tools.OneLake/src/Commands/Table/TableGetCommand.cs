// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;

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
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            var ns = !string.IsNullOrWhiteSpace(options.Namespace) ? options.Namespace : options.Schema!;

            var tableResult = await _oneLakeService.GetTableAsync(workspaceIdentifier!, itemIdentifier!, ns, options.Table!, cancellationToken);
            var result = new TableGetCommandResult(tableResult.Workspace, tableResult.Item, tableResult.Namespace, tableResult.Table, tableResult.Definition, tableResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}, Table: {Table}.", options.WorkspaceId, options.ItemId, options.Table);
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
