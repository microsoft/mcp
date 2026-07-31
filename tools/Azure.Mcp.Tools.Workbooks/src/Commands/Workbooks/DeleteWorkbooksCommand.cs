// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

[CommandMetadata(
    Id = "17bb94ef-9df1-45d2-a1a0-ed57656ca067",
    Name = "delete",
    Title = "Delete Workbook",
    Description = """
        Delete one or more workbooks by their Azure resource IDs.
        This command soft deletes workbooks: they will be retained for 90 days.
        If needed, you can restore them from the Recycle Bin through the Azure Portal.

        BATCH: Accepts multiple --workbook-ids values. Partial failures are reported per-workbook.
        Individual failures do not fail the entire batch operation.

        To learn more, visit: https://learn.microsoft.com/azure/azure-monitor/visualize/workbooks-manage
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class DeleteWorkbooksCommand(ILogger<DeleteWorkbooksCommand> logger, IWorkbooksService workbooksService)
    : AuthenticatedCommand<DeleteWorkbookOptions, DeleteWorkbooksCommand.DeleteWorkbooksCommandResult>
{
    private readonly ILogger<DeleteWorkbooksCommand> _logger = logger;
    private readonly IWorkbooksService _workbooksService = workbooksService;

    public override void ValidateOptions(DeleteWorkbookOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (options.WorkbookIds == null || options.WorkbookIds.Length == 0)
        {
            validationResult.Errors.Add("At least one workbook ID is required");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, DeleteWorkbookOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workbooksService.DeleteWorkbooksAsync(
                options.WorkbookIds,
                options.RetryPolicy,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result.Succeeded.ToList(), result.Failed.ToList()),
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
