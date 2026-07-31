// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

[CommandMetadata(
    Id = "1f55cab9-0bbb-499a-a9ac-1492f11c043a",
    Name = "list",
    Title = "List SQL Server Firewall Rules",
    Description = """
        Gets/retrieves a list of all firewall rules configured for a SQL server, including their IP address ranges
        and rule names. Returns an array of firewall rule objects with their properties.
        """,
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class FirewallRuleListCommand(ISqlService sqlService, ILogger<FirewallRuleListCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FirewallRuleListOptions, FirewallRuleListCommand.FirewallRuleListResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<FirewallRuleListCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FirewallRuleListOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var firewallRules = await _sqlService.ListFirewallRulesAsync(
                options.Server,
                options.ResourceGroup,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(firewallRules ?? []), SqlJsonContext.Default.FirewallRuleListResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL server firewall rules. Server: {Server}, ResourceGroup: {ResourceGroup}.",
                options.Server, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed accessing the SQL server. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record FirewallRuleListResult(List<SqlServerFirewallRule> FirewallRules);
}
