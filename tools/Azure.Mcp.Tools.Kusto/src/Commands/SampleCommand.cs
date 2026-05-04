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
    Id = "41daed5c-bf44-4cdf-9f3c-1df775465e53",
    Name = "sample",
    Title = "Sample Kusto Table Data",
    Description = "Return a sample of rows from a specific table in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri (or --cluster and --subscription), --database, and --table. Optionally specify --chart-type to receive a rendered chart image instead of the raw JSON results (the JSON is omitted when an image is returned).",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SampleCommand(ILogger<SampleCommand> logger, IKustoService kustoService, IKustoChartRenderer chartRenderer) : BaseTableCommand<SampleOptions>
{
    private readonly ILogger<SampleCommand> _logger = logger;
    private readonly IKustoService _kustoService = kustoService;
    private readonly IKustoChartRenderer _chartRenderer = chartRenderer;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KustoOptionDefinitions.Limit);
        command.Options.Add(KustoOptionDefinitions.ChartType);
        KustoOptionDefinitions.AddChartTypeValidator(command);
    }

    protected override SampleOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Limit = parseResult.GetValueOrDefault<int>(KustoOptionDefinitions.Limit.Name);
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
            List<JsonElement> results;
            // Validate limit is within safe bounds to prevent resource abuse
            var safeLimit = Math.Clamp(options.Limit ?? 10, 1, 10000);

            var query = $"{KustoService.EscapeKqlIdentifier(options.Table!)} | sample {safeLimit}";

            if (UseClusterUri(options))
            {
                results = await _kustoService.QueryItemsAsync(
                    options.ClusterUri!,
                    options.Database!,
                    query,
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
                    query,
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
                // error so the caller knows exactly why and can adjust their request.
                var image = _chartRenderer.Render(
                    results ?? [],
                    options.ChartType.Value,
                    title: $"Chart of Kusto table sample ({options.ChartType.Value})");
                context.Response.Images = [image];
                context.Response.OmitTextContent = true;
            }
            else
            {
                context.Response.Results = ResponseResult.Create(new(results ?? []), KustoJsonContext.Default.SampleCommandResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred sampling table. Cluster: {Cluster}, Database: {Database}, Table: {Table}.", options.ClusterUri ?? options.ClusterName, options.Database, options.Table);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record SampleCommandResult(List<JsonElement> Results);
}
