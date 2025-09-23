// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Net;
using Azure;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Sql.Commands;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.Server;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.Server;

public sealed class ServerListCommand(ILogger<ServerListCommand> logger)
    : BaseSqlCommand<ServerListOptions>(logger)
{
    private const string CommandTitle = "List SQL Servers";

    public override string Name => "list";

    public override string Description =>
        """
        Lists Azure SQL servers within a resource group including fully qualified domain name, state,
        administrator login, and network access settings. Use this command to discover SQL servers,
        audit configurations, or verify deployment targets. Equivalent to 'az sql server list'.
        Required parameters: subscription ID and resource group name.
        Returns: JSON array of SQL server objects with metadata, network configuration, and tags.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        // Only register subscription and resource group options, not server option
        // since we're listing all servers in the resource group
        command.Options.Add(OptionDefinitions.Common.Subscription.AsRequired());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
    }

    protected override ServerListOptions BindOptions(ParseResult parseResult)
    {
        var options = new ServerListOptions();
        options.Subscription = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        // Server property is inherited from BaseSqlOptions but not needed for listing
        options.Server = null;
        return options;
    }

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

            var servers = await sqlService.ListServersAsync(
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new ServerListResult(servers ?? []),
                SqlJsonContext.Default.ServerListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL servers. ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.ResourceGroup,
                options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed listing SQL servers. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "No SQL servers found for the specified resource group. Verify the resource group and subscription.",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    internal record ServerListResult(List<SqlServer> Servers);
}
