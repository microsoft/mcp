// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

[CommandMetadata(
    Id = "f13fc5d2-7547-480b-a704-36120e2e9b92",
    Name = "delete",
    Title = "Delete SQL Server Firewall Rule",
    Description = """
        Deletes a firewall rule from a SQL server, potentially restricting access for the IP addresses that were
        previously allowed by this rule. The operation is idempotent - if the rule doesn't exist, no error is returned.
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FirewallRuleDeleteCommand(ISqlService sqlService, ILogger<FirewallRuleDeleteCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FirewallRuleDeleteOptions, FirewallRuleDeleteCommand.FirewallRuleDeleteResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<FirewallRuleDeleteCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FirewallRuleDeleteOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _sqlService.DeleteFirewallRuleAsync(
                options.Server,
                options.ResourceGroup,
                options.Subscription!,
                options.FirewallRuleName,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(deleted, options.FirewallRuleName!), SqlJsonContext.Default.FirewallRuleDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Rule: {Rule}.",
                options.Server, options.ResourceGroup, options.FirewallRuleName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server or firewall rule not found. Verify the server name, rule name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed deleting the firewall rule. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    public sealed record FirewallRuleDeleteResult(bool Deleted, string RuleName);
}
