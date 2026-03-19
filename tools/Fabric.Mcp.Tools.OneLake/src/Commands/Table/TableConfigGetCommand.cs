// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.OneLake.Models;
using Fabric.Mcp.Tools.OneLake.Options;
using Fabric.Mcp.Tools.OneLake.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Fabric.Mcp.Tools.OneLake.Commands.Table;

public sealed class TableConfigGetCommand(
    ILogger<TableConfigGetCommand> logger,
    IOneLakeService oneLakeService) : BaseItemCommand<BaseItemOptions>()
{
    private readonly ILogger<TableConfigGetCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IOneLakeService _oneLakeService = oneLakeService ?? throw new ArgumentNullException(nameof(oneLakeService));

    public override string Id => "bc15c475-0329-4cc3-aaa8-0e9f3fbde6f8";
    public override string Name => "get-table-config";
    public override string Title => "Get OneLake Table Configuration";
    public override string Description => "Retrieves table API configuration for OneLake. Use this when the user needs to understand table access settings.";

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
            var configuration = await _oneLakeService.GetTableConfigurationAsync(options.WorkspaceId!, options.ItemId!, cancellationToken);
            var result = new TableConfigGetCommandResult(configuration.Workspace, configuration.Item, configuration.Configuration, configuration.RawResponse);

            context.Response.Results = ResponseResult.Create(result, OneLakeJsonContext.Default.TableConfigGetCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table configuration. Options: {@Options}", options);
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
