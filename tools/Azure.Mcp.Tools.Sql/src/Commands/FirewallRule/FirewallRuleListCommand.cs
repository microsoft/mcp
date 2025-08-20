// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Services.Telemetry;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

public sealed class FirewallRuleListCommand(ILogger<FirewallRuleListCommand> logger)
    : BaseSqlCommand<FirewallRuleListOptions>(logger)
{
    private const string CommandTitle = "List SQL Server Firewall Rules";

    public override string Name => "list";

    public override string Description =>
        """
        Gets a list of firewall rules for a SQL server. This command retrieves all 
        firewall rules configured for the specified SQL server, including their IP address ranges
        and rule names. Returns an array of firewall rule objects with their properties.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            var sqlService = context.GetService<ISqlService>();

            var firewallRules = await sqlService.ListFirewallRulesAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy);

            context.Response.Results = firewallRules?.Count > 0
                ? ResponseResult.Create(
                    new FirewallRuleListResult(firewallRules),
                    SqlJsonContext.Default.FirewallRuleListResult)
                : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL server firewall rules. Server: {Server}, ResourceGroup: {ResourceGroup}, Options: {@Options}",
                options.Server, options.ResourceGroup, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    internal record FirewallRuleListResult(List<SqlServerFirewallRule> FirewallRules);
}
