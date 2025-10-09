// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Kusto.Commands;

public sealed class SampleCommand(ILogger<SampleCommand> logger) : BaseTableCommand<SampleOptions>
{
    private const string CommandTitle = "Sample Kusto Table Data";
    private readonly ILogger<SampleCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(KustoOptionDefinitions.Limit);
    }

    protected override SampleOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Limit = parseResult.GetValueOrDefault<int>(KustoOptionDefinitions.Limit.Name);
        return options;
    }

    public override string Name => "sample";

    public override string Description =>
        "Return a sample of rows from a specific table in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri (or --cluster and --subscription), --database, and --table.";

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var kusto = context.GetService<IKustoService>();
            List<JsonElement> results;
            var query = $"{options.Table} | sample {options.Limit}";

            if (UseClusterUri(options))
            {
                results = await kusto.QueryItemsAsync(
                    options.ClusterUri!,
                    options.Database!,
                    query,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }
            else
            {
                results = await kusto.QueryItemsAsync(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Database!,
                    query,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }

            context.Response.Results = ResponseResult.Create(new(results ?? []), KustoJsonContext.Default.SampleCommandResult);
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
