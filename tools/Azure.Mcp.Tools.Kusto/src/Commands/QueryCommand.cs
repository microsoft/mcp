// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

public sealed class QueryCommand(ILogger<QueryCommand> logger) : BaseDatabaseCommand<QueryOptions>()
{
    private const string CommandTitle = "Query Kusto Database";
    private readonly ILogger<QueryCommand> _logger = logger;

    public override string Id => "d1e22074-53ce-4eef-8596-0ea134a9e317";

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KustoOptionDefinitions.Query);
        command.Options.Add(KustoOptionDefinitions.ShowStats);
    }

    protected override QueryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Query = parseResult.GetValueOrDefault<string>(KustoOptionDefinitions.Query.Name);
        options.ShowStats = parseResult.GetValueOrDefault<bool>(KustoOptionDefinitions.ShowStats.Name);
        return options;
    }

    public override string Name => "query";

    public override string Description =>
        "Executes a query against an Azure Data Explorer/Kusto/KQL cluster to search for specific terms, retrieve records, or perform management operations. Required: --cluster-uri (or --cluster and --subscription), --database, and --query. Optional: --show-stats to include execution statistics.";

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            (List<JsonElement> Items, JsonElement? Statistics) queryResult;
            var kusto = context.GetService<IKustoService>();
            string? clusterUri;

            if (UseClusterUri(options))
            {
                clusterUri = options.ClusterUri!;
                queryResult = await kusto.QueryItemsWithStatisticsAsync(
                    clusterUri,
                    options.Database!,
                    options.Query!,
                    options.ShowStats,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                clusterUri = null;
                queryResult = await kusto.QueryItemsWithStatisticsAsync(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Database!,
                    options.Query!,
                    options.ShowStats,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }

            var webExplorerUrl = clusterUri is not null
                ? DeeplinkHelper.BuildWebExplorerUrl(clusterUri, options.Database!, options.Query!)
                : null;

            context.Response.Results = ResponseResult.Create(
                new(queryResult.Items ?? [], queryResult.Statistics, webExplorerUrl),
                KustoJsonContext.Default.QueryCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred querying Kusto. Cluster: {Cluster}, Database: {Database},"
            + " Query: {Query}", options.ClusterUri ?? options.ClusterName, options.Database, options.Query);
            HandleException(context, ex);
        }
        return context.Response;
    }

    internal record QueryCommandResult(
        List<JsonElement> Items,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] JsonElement? Statistics = null,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? WebExplorerUrl = null);
}
