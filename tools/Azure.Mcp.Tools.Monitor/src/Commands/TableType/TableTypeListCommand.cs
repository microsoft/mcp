// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Monitor.Options.TableType;
using Azure.Mcp.Tools.Monitor.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Monitor.Commands.TableType;

public sealed class TableTypeListCommand(ILogger<TableTypeListCommand> logger) : BaseWorkspaceMonitorCommand<TableTypeListOptions>()
{
    private const string CommandTitle = "List Log Analytics Table Types";
    private readonly ILogger<TableTypeListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        "List available table types in a Log Analytics workspace. Returns table type names.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var monitorService = context.GetService<IMonitorService>();
            var tableTypes = await monitorService.ListTableTypes(
                options.Subscription!,
                options.ResourceGroup!,
                options.Workspace!,
                options.Tenant,
                options.RetryPolicy);

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
