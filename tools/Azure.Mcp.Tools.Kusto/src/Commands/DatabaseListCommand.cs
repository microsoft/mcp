// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Kusto.Commands;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger) : BaseClusterCommand<DatabaseListOptions>()
{
    private const string CommandTitle = "List Kusto Databases";
    private readonly ILogger<DatabaseListCommand> _logger = logger;

    public override string Name => "list";

    public override string Description =>
        """
        List all databases in a Kusto cluster.
        Requires `cluster-uri` ( or `subscription` and `cluster`).
        Result is a list of database names, returned as a JSON array.
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

            List<string> databasesNames = [];

            if (UseClusterUri(options))
            {
                databasesNames = await kusto.ListDatabases(
                    options.ClusterUri!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }
            else
            {
                databasesNames = await kusto.ListDatabases(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy);
            }

            context.Response.Results = ResponseResult.Create(new(databasesNames ?? []), KustoJsonContext.Default.DatabaseListCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred listing databases. Cluster: {Cluster}.", options.ClusterUri ?? options.ClusterName);
            HandleException(context, ex);
        }
        return context.Response;
    }

    public record DatabaseListCommandResult(List<string> Databases);
}
