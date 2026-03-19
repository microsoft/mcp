// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Option;

namespace Fabric.Mcp.Tools.OneLake.Commands.Table;

public sealed class TableListCommand(
    ILogger<TableListCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<TableListOptions>()
{
    private readonly ILogger<TableListCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "7b1688e5-2a16-475d-8fd1-9bf3b0acf4f7";
    public override string Name => "list-tables";
    public override string Title => "List OneLake Tables";
    public override string Description => "Lists tables in OneLake. Use this when the user needs to see available tables.";

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
        command.Options.Add(FabricOptionDefinitions.Namespace.AsOptional());
        command.Options.Add(FabricOptionDefinitions.Schema.AsOptional());
    }

    protected override TableListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Namespace = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Namespace.Name);
        options.Namespace ??= parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Schema.Name);
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
            var tablesResult = await _oneLakeService.ListTablesAsync(options.WorkspaceId!, options.ItemId!, options.Namespace!, cancellationToken);
            var result = new TableListCommandResult(tablesResult.Workspace, tablesResult.Item, tablesResult.Namespace, tablesResult.Tables, tablesResult.RawResponse);
            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing tables. Options: {@Options}", options);
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
