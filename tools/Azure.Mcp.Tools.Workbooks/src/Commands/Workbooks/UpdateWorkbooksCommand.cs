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
    Id = "9efdc32c-22bc-4b85-8b5c-2fbefc0e927e",
    Name = "update",
    Title = "Update Workbook",
    Description = "Updates properties of an existing Azure Workbook by adding new steps, modifying content, or changing the display name. Returns the updated workbook details.  Requires the workbook resource ID and either new serialized content or a new display name.",
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class UpdateWorkbooksCommand(ILogger<UpdateWorkbooksCommand> logger, IWorkbooksService workbooksService)
    : AuthenticatedCommand<UpdateWorkbooksOptions, UpdateWorkbooksCommand.UpdateWorkbooksCommandResult>
{
    private readonly ILogger<UpdateWorkbooksCommand> _logger = logger;
    private readonly IWorkbooksService _workbooksService = workbooksService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, UpdateWorkbooksOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var updatedWorkbook = await _workbooksService.UpdateWorkbookAsync(
                options.WorkbookId,
                options.DisplayName,
                options.SerializedContent,
                options.RetryPolicy,
                options.Tenant,
                cancellationToken) ?? throw new InvalidOperationException("Failed to update workbook");

            context.Response.Results = ResponseResult.Create(new(updatedWorkbook), WorkbooksJsonContext.Default.UpdateWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workbook with ID: {WorkbookId}", options.WorkbookId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record UpdateWorkbooksCommandResult(WorkbookInfo Workbook);
}
