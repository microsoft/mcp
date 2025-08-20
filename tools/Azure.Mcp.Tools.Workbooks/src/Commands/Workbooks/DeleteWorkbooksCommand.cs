// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class DeleteWorkbooksCommand(ILogger<DeleteWorkbooksCommand> logger) : GlobalCommand<DeleteWorkbookOptions>
{
    private const string CommandTitle = "Delete Workbook";
    private readonly ILogger<DeleteWorkbooksCommand> _logger = logger;

    private static readonly Option<string> _workbookIdOption = WorkbooksOptionDefinitions.WorkbookId;

    public override string Name => "delete";

    public override string Description =>
        """
        Delete a workbook by its Azure resource ID. 
        This command soft deletes the workbook: it will be retained for 90 days.
        If needed, you can restore it from the Recycle Bin through the Azure Portal.
        
        To learn more, visit: https://learn.microsoft.com/azure/azure-monitor/visualize/workbooks-manage
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = true, ReadOnly = false };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_workbookIdOption);
    }

    protected override DeleteWorkbookOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkbookId = parseResult.GetValueForOption(_workbookIdOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var workbooksService = context.GetService<IWorkbooksService>();
            var deleted = await workbooksService.DeleteWorkbook(options.WorkbookId!, options.RetryPolicy, options.Tenant);

            if (deleted)
            {
                context.Response.Results = ResponseResult.Create(
                            new DeleteWorkbooksCommandResult(options.WorkbookId!, "Successfully deleted"),
                            WorkbooksJsonContext.Default.DeleteWorkbooksCommandResult);
            }
            else
            {
                throw new InvalidOperationException($"Failed to delete workbook with ID '{options.WorkbookId}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workbook with ID: {WorkbookId}", options.WorkbookId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public sealed record DeleteWorkbooksCommandResult(string WorkbookId, string Message);
}
