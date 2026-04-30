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
    Description = "Executes a query against an Azure Data Explorer/Kusto/KQL cluster to search for specific terms, retrieve records, or perform management operations. Required: --cluster-uri (or --cluster and --subscription), --database, and --query. Optionally specify --chart-type to receive a rendered chart image alongside the raw JSON results.",
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
    }

    protected override QueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.Query.Name);
        var chartTypeStr = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.ChartType.Name);
        options.ChartType = Enum.TryParse<ChartType>(chartTypeStr, ignoreCase: true, out var ct) ? ct : (ChartType?)null;
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

            context.Response.Results = ResponseResult.Create(new(results ?? []), KustoJsonContext.Default.QueryCommandResult);

            if (options.ChartType.HasValue && results is { Count: > 1 })
            {
                var image = _chartRenderer.TryRender(results, options.ChartType.Value, title: $"Chart of Kusto query results ({options.ChartType.Value})");
                if (image is not null)
                {
                    context.Response.Images = [image];
                    context.Response.Results = null;
                }
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
