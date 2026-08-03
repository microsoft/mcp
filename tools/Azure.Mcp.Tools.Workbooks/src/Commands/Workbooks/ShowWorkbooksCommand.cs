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
    Id = "a7a882cd-1729-49ed-b349-2a79f8c7de56",
    Name = "show",
    Title = "Get Workbook",
    Description = """
        Retrieve full workbook details via ARM API (includes serializedData content).

        USE FOR: Getting complete workbook definition including visualization JSON.
        RETURNS: Full workbook properties, serializedData, tags, etag.

        BATCH: Accepts multiple --workbook-ids values. Partial failures reported per-workbook.
        PERFORMANCE: Use 'list' first for discovery, then 'show' for specific workbooks.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ShowWorkbooksCommand(ILogger<ShowWorkbooksCommand> logger, IWorkbooksService workbooksService)
    : AuthenticatedCommand<ShowWorkbooksOptions, ShowWorkbooksCommand.ShowWorkbooksCommandResult>
{
    private readonly ILogger<ShowWorkbooksCommand> _logger = logger;
    private readonly IWorkbooksService _workbooksService = workbooksService;

    public override void ValidateOptions(ShowWorkbooksOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);
        if (options.WorkbookIds == null || options.WorkbookIds.Length == 0)
        {
            validationResult.Errors.Add("At least one workbook ID is required");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ShowWorkbooksOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workbooksService.GetWorkbooksAsync(
                options.WorkbookIds,
                options.RetryPolicy,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result.Succeeded.ToList(), result.Failed.ToList()),
                WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workbooks");
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record ShowWorkbooksCommandResult(List<WorkbookInfo> Workbooks, List<WorkbookError> Errors);
}
