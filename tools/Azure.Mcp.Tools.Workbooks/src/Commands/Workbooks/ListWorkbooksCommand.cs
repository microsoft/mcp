// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class ListWorkbooksCommand(ILogger<ListWorkbooksCommand> logger) : SubscriptionCommand<ListWorkbooksOptions>
{
    private const string CommandTitle = "List Workbooks";
    private readonly ILogger<ListWorkbooksCommand> _logger = logger;

    private readonly Option<string> _kindOption = WorkbooksOptionDefinitions.Kind;
    private readonly Option<string> _categoryOption = WorkbooksOptionDefinitions.Category;
    private readonly Option<string> _sourceIdOption = WorkbooksOptionDefinitions.SourceIdFilter;

    public override string Name => "list";

    public override string Description =>
        """
        List all workbooks in a specific resource group. This command retrieves all workbooks available
        in the specified resource group within the given subscription. Resource group is required.
        Optionally filter by kind (shared/user), category (workbook/sentinel/etc), or source resource ID.
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
        RequireResourceGroup();
        command.Options.Add(_kindOption);
        command.Options.Add(_categoryOption);
        command.Options.Add(_sourceIdOption);
    }

    protected override ListWorkbooksOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Kind = parseResult.GetValueOrDefault(_kindOption);
        options.Category = parseResult.GetValueOrDefault(_categoryOption);
        options.SourceId = parseResult.GetValueOrDefault(_sourceIdOption);
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
            var filters = options.ToFilters();
            var workbooks = await workbooksService.ListWorkbooks(
                options.Subscription!,
                options.ResourceGroup!,
                filters,
                options.RetryPolicy,
                options.Tenant);

            context.Response.Results = workbooks?.Count > 0
                ? ResponseResult.Create(new ListWorkbooksCommandResult(workbooks), WorkbooksJsonContext.Default.ListWorkbooksCommandResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workbooks for subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ListWorkbooksCommandResult(List<WorkbookInfo> Workbooks);
}
