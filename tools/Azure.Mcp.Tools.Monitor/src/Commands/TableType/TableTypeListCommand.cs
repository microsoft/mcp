// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Options.TableType;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.TableType;

public sealed class TableTypeListCommand(ILogger<TableTypeListCommand> logger, IMonitorService monitorService) : BaseWorkspaceMonitorCommand<TableTypeListOptions, TableTypeListCommand.TableTypeListCommandResult>()
{
    protected override JsonTypeInfo<TableTypeListCommandResult> ResultTypeInfo => MonitorJsonContext.Default.TableTypeListCommandResult;
    private const string CommandTitle = "List Log Analytics Table Types";
    private readonly ILogger<TableTypeListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

    public override string Id => "17928c13-3907-428c-8232-74f7aec1d76d";

    public override string Name => "list";

    public override string Description =>
        "List available table types in a Log Analytics workspace. Returns table type names.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var tableTypes = await _monitorService.ListTableTypes(
                options.Subscription!,
                options.ResourceGroup!,
                options.Workspace!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(tableTypes ?? []));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing table types.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record TableTypeListCommandResult(List<string> TableTypes);
}
