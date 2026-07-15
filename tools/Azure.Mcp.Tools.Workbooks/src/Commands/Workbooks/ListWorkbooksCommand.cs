// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

[CommandMetadata(
    Id = "c4c90435-fbc0-4598-ba82-3b9213d58b26",
    Name = "list",
    Title = "List Workbooks",
    Description = """
        Search Azure Workbooks using Resource Graph (fast metadata query).

        USE FOR: Discovery, filtering, counting workbooks across scopes.
        RETURNS: Workbook metadata (id, name, location, category, timestamps).
        DOES NOT RETURN: Full workbook content (serializedData) by default - use 'show' for that or set --output-format=full.

        SCOPE: By default searches workbooks in your current Azure context (tenant/subscription). Use --subscription and --resource-group to explicitly control scope.
        TOTAL COUNT: Returns server-side total count by default (not just returned items).
        MAX RESULTS: Default 50, max 1000. Use --max-results to adjust.
        OUTPUT FORMAT: Use --output-format=summary for minimal tokens, --output-format=full for serializedData.

        FILTERS: --name-contains, --category, --kind, --source-id, --modified-after for semantic filtering.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class ListWorkbooksCommand(ILogger<ListWorkbooksCommand> logger, IWorkbooksService workbooksService, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<ListWorkbooksOptions, ListWorkbooksCommand.ListWorkbooksCommandResult>(subscriptionResolver)
{
    private readonly ILogger<ListWorkbooksCommand> _logger = logger;
    private readonly IWorkbooksService _workbooksService = workbooksService;

    public override void PostBindOptions(ListWorkbooksOptions options)
    {
        base.PostBindOptions(options);
        if (!string.IsNullOrEmpty(options.ModifiedAfter) && DateTimeOffset.TryParse(options.ModifiedAfter, out var modifiedAfter))
        {
            options.ParsedModifiedAfter = modifiedAfter;
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ListWorkbooksOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var filters = options.ToFilters();

            var result = await _workbooksService.ListWorkbooksAsync(
                string.IsNullOrEmpty(options.Subscription) ? null : [options.Subscription],
                string.IsNullOrEmpty(options.ResourceGroup) ? null : [options.ResourceGroup],
                filters,
                options.MaxResults == null || options.MaxResults.Value < 1 ? 50 : Math.Min(options.MaxResults.Value, 1000),
                options.IncludeTotalCount ?? true,
                ParseOutputFormat(options.OutputFormat),
                options.RetryPolicy,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new(result.Workbooks.ToList(), result.TotalCount, result.Workbooks.Count),
                WorkbooksJsonContext.Default.ListWorkbooksCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing workbooks for subscription: {Subscription}", options.Subscription);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static OutputFormat ParseOutputFormat(string? format)
    {
        return format?.ToLowerInvariant() switch
        {
            "summary" => OutputFormat.Summary,
            "full" => OutputFormat.Full,
            _ => OutputFormat.Standard
        };
    }

    public sealed record ListWorkbooksCommandResult(List<WorkbookInfo> Workbooks, int? TotalCount, int Returned);
}
