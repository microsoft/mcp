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
public sealed class TableListCommand(
    ILogger<TableListCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<TableListOptions, TableListCommand.TableListCommandResult>
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
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            var ns = !string.IsNullOrWhiteSpace(options.Namespace) ? options.Namespace : options.Schema!;

            var tablesResult = await _oneLakeService.ListTablesAsync(workspaceIdentifier!, itemIdentifier!, ns, cancellationToken);
            var result = new TableListCommandResult(tablesResult.Workspace, tablesResult.Item, tablesResult.Namespace, tablesResult.Tables, tablesResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}, Namespace: {Namespace}.", options.WorkspaceId, options.ItemId, options.Namespace);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class TableListCommandResult
    {
        public string Workspace { get; init; } = string.Empty;
        public string Item { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public JsonElement Tables { get; init; } = default;
        public string RawResponse { get; init; } = string.Empty;

        public TableListCommandResult()
        {
        }

        public TableListCommandResult(string workspace, string item, string namespaceName, JsonElement tables, string rawResponse)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Namespace = namespaceName ?? throw new ArgumentNullException(nameof(namespaceName));
            Tables = tables;
            RawResponse = rawResponse ?? string.Empty;
        }
    }
}
