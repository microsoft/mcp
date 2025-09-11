// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class UpdateWorkbooksCommand(ILogger<UpdateWorkbooksCommand> logger) : BaseWorkbooksCommand<UpdateWorkbooksOptions>
{
    private const string CommandTitle = "Update Workbook";
    private readonly ILogger<UpdateWorkbooksCommand> _logger = logger;
    private readonly Option<string> _workbookIdOption = WorkbooksOptionDefinitions.WorkbookId;
    private readonly Option<string> _displayNameOption = WorkbooksOptionDefinitions.DisplayName;
    private readonly Option<string> _serializedContentOption = WorkbooksOptionDefinitions.SerializedContent;

    public override string Name => "update";

    public override string Description =>
        """
        Updates properties of a workbook, including its display name and serialized content.
        At least one property must be provided for the update operation.
        Returns the updated workbook object upon successful completion.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_workbookIdOption);
        command.Options.Add(_displayNameOption);
        command.Options.Add(_serializedContentOption);
    }

    protected override UpdateWorkbooksOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkbookId = parseResult.GetValueOrDefault(_workbookIdOption);
        options.DisplayName = parseResult.GetValueOrDefault(_displayNameOption);
        options.SerializedContent = parseResult.GetValueOrDefault(_serializedContentOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var workbooksService = context.GetService<IWorkbooksService>();
            var updatedWorkbook = await workbooksService.UpdateWorkbook(
                options.WorkbookId!,
                options.DisplayName,
                options.SerializedContent,
                options.RetryPolicy,
                options.Tenant) ?? throw new InvalidOperationException("Failed to update workbook");

            context.Response.Results = ResponseResult.Create(
                new UpdateWorkbooksCommandResult(updatedWorkbook),
                WorkbooksJsonContext.Default.UpdateWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workbook with ID: {WorkbookId}", options.WorkbookId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record UpdateWorkbooksCommandResult(WorkbookInfo Workbook);
}
