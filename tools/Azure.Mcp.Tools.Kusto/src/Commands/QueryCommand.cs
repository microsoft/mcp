// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Rendering;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

[CommandMetadata(
    Id = "d1e22074-53ce-4eef-8596-0ea134a9e317",
    Name = "query",
    Title = "Query Kusto Database",
    Description = "Executes a query against an Azure Data Explorer/Kusto/KQL cluster to search for specific terms, retrieve records, or perform management operations. Required: --cluster-uri (or --cluster and --subscription), --database, and --query. Optionally specify --chart-type to receive a rendered chart image instead of the raw JSON results (the JSON is omitted when an image is returned). Charts are intended for visual pattern analysis — use them to identify trends, anomalies, spikes, dips, and plateaus over time, not to extract exact values. When a chart is returned, describe what you observe visually (e.g. 'CPU spikes around 14:00', 'steady plateau between 10:00–12:00', 'gradual upward trend') rather than quoting precise numbers.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueryCommand(ILogger<QueryCommand> logger, IKustoService kustoService, IKustoChartRenderer chartRenderer) : BaseDatabaseCommand<QueryOptions>()
{
    private readonly ILogger<QueryCommand> _logger = logger;
    private readonly IKustoService _kustoService = kustoService;
    private readonly IKustoChartRenderer _chartRenderer = chartRenderer;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KustoOptionDefinitions.Query);
        command.Options.Add(KustoOptionDefinitions.ChartType);
        KustoOptionDefinitions.AddChartTypeValidator(command);
    }

    protected override QueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.Query.Name);
        // The option-level validator on KustoOptionDefinitions.ChartType guarantees the value
        // is either absent or a valid ChartType, so Enum.Parse cannot fail here.
        var chartTypeStr = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.ChartType.Name);
        options.ChartType = string.IsNullOrWhiteSpace(chartTypeStr)
            ? null
            : Enum.Parse<ChartType>(chartTypeStr, ignoreCase: true);
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
            List<JsonElement> results = [];

            if (UseClusterUri(options))
            {
                results = await _kustoService.QueryItemsAsync(
                    options.ClusterUri!,
                    options.Database!,
                    options.Query!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                results = await _kustoService.QueryItemsAsync(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Database!,
                    options.Query!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }

            if (options.ChartType.HasValue)
            {
                // The user explicitly opted in to chart rendering; the renderer throws a
                // ChartRenderingException with a descriptive message if the data shape doesn't
                // match the requested chart type, which HandleException turns into a tool-call
                // error so the caller knows exactly why and can adjust their query.
                var image = _chartRenderer.Render(
                    results ?? [],
                    options.ChartType.Value,
                    title: $"Chart of Kusto query results ({options.ChartType.Value})");
                context.Response.Images = [image];
                context.Response.Message = "Query results rendered as a chart image — see the attached image. "
                    + "Use it to identify visual patterns: trends, anomalies, spikes, dips, and plateaus. "
                    + "Describe what you observe (e.g. 'spike around 14:00', 'steady plateau', 'gradual upward trend') "
                    + "rather than quoting precise values — approximate ranges are sufficient.";
                context.Response.OmitTextContent = true;
            }
            else
            {
                context.Response.Results = ResponseResult.Create(new(results ?? []), KustoJsonContext.Default.QueryCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred querying Kusto. Cluster: {Cluster}, Database: {Database},"
            + " Query: {Query}", options.ClusterUri ?? options.ClusterName, options.Database, options.Query);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record QueryCommandResult(List<JsonElement> Items);
}
