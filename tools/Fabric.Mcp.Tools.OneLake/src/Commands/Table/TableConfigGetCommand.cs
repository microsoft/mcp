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
    Id = "bc15c475-0329-4cc3-aaa8-0e9f3fbde6f8",
    Name = "get_table_config",
    Title = "Get OneLake Table Configuration",
    Description = "Retrieves table API configuration for OneLake. Use this when the user needs to understand table access settings.",
    Destructive = false,
    Idempotent = true,
    LocalRequired = false,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false)]
public sealed class TableConfigGetCommand(
    ILogger<TableConfigGetCommand> logger,
    IOneLakeService oneLakeService) : AuthenticatedCommand<TableConfigGetOptions, TableConfigGetCommand.TableConfigGetCommandResult>
{
    private readonly ILogger<TableConfigGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override void ValidateOptions(TableConfigGetOptions options, ValidationResult validationResult)
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, TableConfigGetOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var workspaceIdentifier = !string.IsNullOrWhiteSpace(options.WorkspaceId)
                ? options.WorkspaceId
                : options.Workspace;

            var itemIdentifier = !string.IsNullOrWhiteSpace(options.ItemId)
                ? options.ItemId
                : options.Item;

            var configuration = await _oneLakeService.GetTableConfigurationAsync(workspaceIdentifier!, itemIdentifier!, cancellationToken);
            var result = new TableConfigGetCommandResult(configuration.Workspace, configuration.Item, configuration.Configuration, configuration.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableConfigGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table configuration. WorkspaceId: {WorkspaceId}, ItemId: {ItemId}.", options.WorkspaceId, options.ItemId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed class TableConfigGetCommandResult
    {
        public string Workspace { get; init; } = string.Empty;
        public string Item { get; init; } = string.Empty;
        public JsonElement Configuration { get; init; } = default;
        public string RawResponse { get; init; } = string.Empty;

        public TableConfigGetCommandResult()
        {
        }

        public TableConfigGetCommandResult(string workspace, string item, JsonElement configuration, string rawResponse)
        {
            Workspace = workspace ?? throw new ArgumentNullException(nameof(workspace));
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Configuration = configuration;
            RawResponse = rawResponse ?? string.Empty;
        }
    }
}
