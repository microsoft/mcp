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

    private readonly Option<int> _limitOption = KustoOptionDefinitions.Limit;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_limitOption);
    }

    protected override SampleOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Limit = parseResult.GetValueOrDefault(_limitOption);
        return options;
    }

    public override string Name => "sample";

    public override string Description =>
        """
        Return a sample of rows from the specified table in an Kusto table.
        Requires `cluster-uri` (or `cluster`), `database`, and `table`.
        Results are returned as a JSON array of documents, for example: `[{'Column1': val1, 'Column2': val2}, ...]`.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

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
                results = await kusto.QueryItems(
                    options.ClusterUri!,
                    options.Database!,
                    query,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }
            else
            {
                results = await kusto.QueryItems(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Database!,
                    query,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }

            context.Response.Results = results?.Count > 0 ?
                ResponseResult.Create(new SampleCommandResult(results), KustoJsonContext.Default.SampleCommandResult) :
                null;
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
