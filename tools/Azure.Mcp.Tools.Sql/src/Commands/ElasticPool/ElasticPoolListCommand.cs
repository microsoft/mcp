// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.ElasticPool;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.ElasticPool;

public sealed class ElasticPoolListCommand(ILogger<ElasticPoolListCommand> logger)
    : BaseElasticPoolCommand<ElasticPoolListOptions>(logger)
{
    private const string CommandTitle = "List SQL Elastic Pools";

    public override string Name => "list";

    public override string Description =>
        """
        Lists all SQL elastic pools in an Azure SQL Server with their SKU, capacity, state, and database limits.
        Use when you need to: view elastic pool inventory, check pool utilization, compare pool configurations,
        or find available pools for database placement.
        Requires: subscription ID, resource group name, server name.
        Returns: JSON array of elastic pools with complete configuration details.
        Equivalent to 'az sql elastic-pool list'.
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
            var sqlService = context.GetService<ISqlService>();

            var elasticPools = await sqlService.GetElasticPoolsAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(elasticPools ?? []), SqlJsonContext.Default.ElasticPoolListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL elastic pools. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record ElasticPoolListResult(List<SqlElasticPool> ElasticPools);
}
