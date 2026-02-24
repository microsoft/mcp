// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Workbooks.Models;
using Azure.Mcp.Tools.Workbooks.Options;
using Azure.Mcp.Tools.Workbooks.Options.Workbook;
using Azure.Mcp.Tools.Workbooks.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.Workbooks.Commands.Workbooks;

public sealed class ListWorkbooksCommand(ILogger<ListWorkbooksCommand> logger) : SubscriptionCommand<ListWorkbooksOptions>
{
    private const string CommandTitle = "List Workbooks";
    private readonly ILogger<ListWorkbooksCommand> _logger = logger;
    public override string Id => "c4c90435-fbc0-4598-ba82-3b9213d58b26";

    public override string Name => "list";

    public override string Description =>
        """
        Search Azure Workbooks using Resource Graph (fast metadata query).

        USE FOR: Discovery, filtering, counting workbooks across scopes.
        RETURNS: Workbook metadata (id, name, location, category, timestamps).
        DOES NOT RETURN: Full workbook content (serializedData) by default - use 'show' for that or set --output-format=full.

        SCOPE: By default searches workbooks in your current Azure context (tenant/subscription). Use --subscription and --resource-group to explicitly control scope.
        TOTAL COUNT: Returns server-side total count by default (not just returned items).
        MAX RESULTS: Default 50, max 1000. Use --max-results to adjust.
        OUTPUT FORMAT: Use --output-format=summary for minimal tokens, --output-format=full for serializedData.

        FILTERS: --name-contains, --category, --kind, --source-id, --modified-after for semantic filtering.
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
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(WorkbooksOptionDefinitions.Kind);
        command.Options.Add(WorkbooksOptionDefinitions.Category);
        command.Options.Add(WorkbooksOptionDefinitions.SourceIdFilter);
        command.Options.Add(WorkbooksOptionDefinitions.NameContains);
        command.Options.Add(WorkbooksOptionDefinitions.ModifiedAfter);
        command.Options.Add(WorkbooksOptionDefinitions.OutputFormat);
        command.Options.Add(WorkbooksOptionDefinitions.MaxResults);
        command.Options.Add(WorkbooksOptionDefinitions.IncludeTotalCount);
    }

    protected override ListWorkbooksOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);

        var resourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.ResourceGroups = string.IsNullOrEmpty(resourceGroup) ? null : [resourceGroup];

        options.Kind = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.Kind.Name);
        options.Category = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.Category.Name);
        options.SourceId = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.SourceIdFilter.Name);
        options.NameContains = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.NameContains.Name);

        var modifiedAfterStr = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.ModifiedAfter.Name);
        if (!string.IsNullOrEmpty(modifiedAfterStr) && DateTimeOffset.TryParse(modifiedAfterStr, out var modifiedAfter))
        {
            options.ModifiedAfter = modifiedAfter;
        }

        var outputFormatStr = parseResult.GetValueOrDefault<string>(WorkbooksOptionDefinitions.OutputFormat.Name);
        options.OutputFormat = ParseOutputFormat(outputFormatStr);

        var maxResults = parseResult.GetValueOrDefault<int>(WorkbooksOptionDefinitions.MaxResults.Name);
        options.MaxResults = maxResults > 0 ? Math.Min(maxResults, 1000) : 50;

        var includeTotalCount = parseResult.GetValueOrDefault<bool?>(WorkbooksOptionDefinitions.IncludeTotalCount.Name);
        options.IncludeTotalCount = includeTotalCount ?? true;

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
            var workbooksService = context.GetService<IWorkbooksService>();
            var filters = options.ToFilters();

            var result = await workbooksService.ListWorkbooksAsync(
                string.IsNullOrEmpty(options.Subscription) ? null : [options.Subscription],
                options.ResourceGroups,
                filters,
                options.MaxResults,
                options.IncludeTotalCount,
                options.OutputFormat,
                options.RetryPolicy,
                options.Tenant,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new ListWorkbooksCommandResult(result.Workbooks.ToList(), result.TotalCount, result.Workbooks.Count),
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

    internal record ListWorkbooksCommandResult(List<WorkbookInfo> Workbooks, int? TotalCount, int Returned);
}
