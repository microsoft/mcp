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

public sealed class ShowWorkbooksCommand(ILogger<ShowWorkbooksCommand> logger) : BaseWorkbooksCommand<ShowWorkbooksOptions>
{
    private const string CommandTitle = "Get Workbook";
    private readonly ILogger<ShowWorkbooksCommand> _logger = logger;

    public override string Name => "show";

    public override string Description =>
        """
        Gets information about a specific workbook by its Azure resource ID.
        Returns workbook details including JSON serialized content, display name, description, category,
        location, kind, tags, version, modification time, and other metadata.
        """;

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

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WorkbooksOptionDefinitions.WorkbookId);
    }

    protected override ShowWorkbooksOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkbookId = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.WorkbookId.Name);
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
            var workbook = await workbooksService.GetWorkbook(options.WorkbookId!, options.RetryPolicy, options.Tenant) ?? throw new InvalidOperationException("Failed to retrieve workbook");

            context.Response.Results = ResponseResult.Create(new(workbook), WorkbooksJsonContext.Default.ShowWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workbook with ID: {WorkbookId}", options.WorkbookId);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ShowWorkbooksCommandResult(WorkbookInfo Workbook);
}
