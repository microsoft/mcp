// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class DeleteWorkbooksCommand(ILogger<DeleteWorkbooksCommand> logger) : GlobalCommand<DeleteWorkbookOptions>
{
    private const string CommandTitle = "Delete Workbook";
    private readonly ILogger<DeleteWorkbooksCommand> _logger = logger;
    public override string Id => "17bb94ef-9df1-45d2-a1a0-ed57656ca067";

    public override string Name => "delete";

    public override string Description =>
        """
        Delete one or more workbooks by their Azure resource IDs.
        This command soft deletes workbooks: they will be retained for 90 days.
        If needed, you can restore them from the Recycle Bin through the Azure Portal.

        BATCH: Accepts multiple --workbook-ids values. Partial failures are reported per-workbook.
        Individual failures do not fail the entire batch operation.

        To learn more, visit: https://learn.microsoft.com/azure/azure-monitor/visualize/workbooks-manage
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WorkbooksOptionDefinitions.WorkbookIds);
    }

    protected override DeleteWorkbookOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkbookIds = parseResult.GetValueOrDefault<string[]>(WorkbooksOptionDefinitions.WorkbookIds.Name);
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
            if (options.WorkbookIds == null || options.WorkbookIds.Length == 0)
            {
                throw new ArgumentException("At least one workbook ID is required");
            }

            var workbooksService = context.GetService<IWorkbooksService>();
            var result = await workbooksService.DeleteWorkbooksAsync(
                options.WorkbookIds,
                options.RetryPolicy,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new DeleteWorkbooksCommandResult(result.Succeeded.ToList(), result.Failed.ToList()),
                WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workbooks");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DeleteWorkbooksCommandResult(List<string> Succeeded, List<WorkbookError> Errors);
}
