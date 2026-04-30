// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Monitor.Options.TableType;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Monitor.Commands.TableType;

[CommandMetadata(
    Id = "17928c13-3907-428c-8232-74f7aec1d76d",
    Name = "list",
    Title = "List Log Analytics Table Types",
    Description = "List available table types in a Log Analytics workspace. Returns table type names.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class TableTypeListCommand(ILogger<TableTypeListCommand> logger, IMonitorService monitorService) : BaseWorkspaceMonitorCommand<TableTypeListOptions>()
{
    private readonly ILogger<TableTypeListCommand> _logger = logger;
    private readonly IMonitorService _monitorService = monitorService;

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

            context.Response.Results = ResponseResult.Create(new(tableTypes ?? []), MonitorJsonContext.Default.TableTypeListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing table types.");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record TableTypeListCommandResult(List<string> TableTypes);
}
