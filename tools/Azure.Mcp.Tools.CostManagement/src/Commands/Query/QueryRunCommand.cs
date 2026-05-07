// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.CostManagement.Models;
using Azure.Mcp.Tools.CostManagement.Options;
using Azure.Mcp.Tools.CostManagement.Options.Query;
using Azure.Mcp.Tools.CostManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.CostManagement.Commands.Query;

[CommandMetadata(
    Id = "f7c4b3a8-9e62-4d18-bc41-2a5d8e6f1b09",
    Name = "run",
    Title = "Query Azure Costs",
    Description = """
        Query actual Azure costs and usage data for a subscription or resource group over a time period.
        Returns aggregated costs in the billing currency, optionally broken down daily and grouped by
        a single Azure dimension. Caller must have 'Cost Management Reader' or 'Reader' on the scope.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class QueryRunCommand(ILogger<QueryRunCommand> logger, ICostManagementService costManagementService)
    : BaseCostManagementCommand<QueryRunOptions>(logger)
{
    private readonly ICostManagementService _costManagementService = costManagementService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CostManagementOptionDefinitions.Timeframe);
        command.Options.Add(CostManagementOptionDefinitions.From);
        command.Options.Add(CostManagementOptionDefinitions.To);
        command.Options.Add(CostManagementOptionDefinitions.Granularity);
        command.Options.Add(CostManagementOptionDefinitions.GroupBy);

        command.Validators.Add(result =>
        {
            var timeframe = result.GetValue(CostManagementOptionDefinitions.Timeframe);
            if (timeframe == QueryTimeframe.Custom)
            {
                var from = result.GetValue(CostManagementOptionDefinitions.From);
                var to = result.GetValue(CostManagementOptionDefinitions.To);
                if (from is null || to is null)
                {
                    result.AddError(
                        "When --timeframe is Custom, both --from and --to must be provided (ISO-8601 dates, UTC).");
                }
                else if (from > to)
                {
                    result.AddError("--from must be earlier than or equal to --to.");
                }
            }
        });
    }

    protected override QueryRunOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Timeframe = parseResult.GetValue(CostManagementOptionDefinitions.Timeframe);
        options.From = parseResult.GetValue(CostManagementOptionDefinitions.From);
        options.To = parseResult.GetValue(CostManagementOptionDefinitions.To);
        options.Granularity = parseResult.GetValue(CostManagementOptionDefinitions.Granularity);
        options.GroupBy = parseResult.GetValue(CostManagementOptionDefinitions.GroupBy);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var timeframe = options.Timeframe ?? QueryTimeframe.MonthToDate;
            var granularity = options.Granularity ?? QueryGranularity.None;

            var result = await _costManagementService.QueryAsync(
                subscription: options.Subscription!,
                resourceGroup: options.ResourceGroup,
                timeframe: timeframe,
                from: options.From,
                to: options.To,
                granularity: granularity,
                groupBy: options.GroupBy,
                tenant: options.Tenant,
                retryPolicy: options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(
                new QueryRunCommandResult(result),
                CostManagementJsonContext.Default.QueryRunCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error querying Cost Management. Subscription: {Subscription}, ResourceGroup: {ResourceGroup}, Timeframe: {Timeframe}, Granularity: {Granularity}, GroupBy: {GroupBy}.",
                options.Subscription, options.ResourceGroup, options.Timeframe, options.Granularity, options.GroupBy);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "Cost data not found. Verify the subscription and resource group exist and are accessible.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed querying Cost Management. The caller needs the 'Cost Management Reader' or 'Reader' role on the subscription or resource group. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.TooManyRequests =>
            $"Cost Management API throttled the request. Retry after the duration specified in the 'x-ms-ratelimit-microsoft.consumption-retry-after' response header. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.BadRequest =>
            $"Cost Management API rejected the query. Common causes: unsupported --group-by dimension; --timeframe value not allowed at this scope (subscription/resource group accept MonthToDate, BillingMonthToDate, TheLastBillingMonth, WeekToDate, Custom); --from later than --to; date range too large. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    internal record QueryRunCommandResult(CostQueryResult Result);
}
