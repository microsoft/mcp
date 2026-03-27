// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.OneLake.Commands.Table;

public sealed class TableNamespaceListCommand(
    ILogger<TableNamespaceListCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<BaseItemOptions>()
{
    private readonly ILogger<TableNamespaceListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "173cfc00-7c12-486d-a0e7-c0d4c1de23fd";
    public override string Name => "list-table-namespaces";
    public override string Title => "List OneLake Table Namespaces";
    public override string Description => "Lists table namespaces in OneLake. Use this when the user needs to discover available table namespaces.";

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
    }

    protected override BaseItemOptions BindOptions(ParseResult parseResult)
    {
        return base.BindOptions(parseResult);
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
            var namespaceResult = await _oneLakeService.ListTableNamespacesAsync(options.WorkspaceId!, options.ItemId!, cancellationToken);
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
