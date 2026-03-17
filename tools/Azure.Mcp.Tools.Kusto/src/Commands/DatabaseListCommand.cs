// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Kusto.Options;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Kusto.Commands;

public sealed class DatabaseListCommand(ILogger<DatabaseListCommand> logger, IKustoService kustoService) : BaseClusterCommand<DatabaseListOptions>()
{
    private const string CommandTitle = "List Kusto Databases";
    private readonly ILogger<DatabaseListCommand> _logger = logger;
    private readonly IKustoService _kustoService = kustoService;

    public override string Id => "0bd79f0b-c360-4c96-b3e0-02fce97dcc41";

    public override string Name => "list";

    public override string Description =>
        "List/enumerate all databases in an Azure Data Explorer/Kusto/KQL cluster. Required: --cluster-uri ( or --cluster and --subscription).";

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
            List<string> databasesNames = [];

            if (UseClusterUri(options))
            {
                databasesNames = await _kustoService.ListDatabasesAsync(
                    options.ClusterUri!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
            }
            else
            {
                databasesNames = await _kustoService.ListDatabasesAsync(
                    options.Subscription!,
                    options.ClusterName!,
                    options.Tenant,
                    options.AuthMethod,
                    options.RetryPolicy,
                    cancellationToken);
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
