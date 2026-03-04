// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class ShowWorkbooksCommand(ILogger<ShowWorkbooksCommand> logger) : BaseWorkbooksCommand<ShowWorkbooksOptions>
{
    private const string CommandTitle = "Get Workbook";
    private readonly ILogger<ShowWorkbooksCommand> _logger = logger;
    public override string Id => "a7a882cd-1729-49ed-b349-2a79f8c7de56";

    public override string Name => "show";

    public override string Description =>
        """
        Retrieve full workbook details via ARM API (includes serializedData content).

        USE FOR: Getting complete workbook definition including visualization JSON.
        RETURNS: Full workbook properties, serializedData, tags, etag.

        BATCH: Accepts multiple --workbook-ids values. Partial failures reported per-workbook.
        PERFORMANCE: Use 'list' first for discovery, then 'show' for specific workbooks.
        """;

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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WorkbooksOptionDefinitions.WorkbookIds);
    }

    protected override ShowWorkbooksOptions BindOptions(ParseResult parseResult)
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
            var result = await workbooksService.GetWorkbooksAsync(
                options.WorkbookIds,
                options.RetryPolicy,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ShowWorkbooksCommandResult(result.Succeeded.ToList(), result.Failed.ToList()),
                WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workbooks");
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ShowWorkbooksCommandResult(List<WorkbookInfo> Workbooks, List<WorkbookError> Errors);
}
