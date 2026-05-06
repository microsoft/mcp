// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization.Metadata;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

public sealed class FirewallRuleListCommand(ISqlService sqlService, ILogger<FirewallRuleListCommand> logger)
    : BaseSqlCommand<FirewallRuleListOptions, FirewallRuleListCommand.FirewallRuleListResult>(logger)
{
    protected override JsonTypeInfo<FirewallRuleListResult> ResultTypeInfo => SqlJsonContext.Default.FirewallRuleListResult;
    private readonly ISqlService _sqlService = sqlService;
    private const string CommandTitle = "List SQL Server Firewall Rules";

    public override string Id => "1f55cab9-0bbb-499a-a9ac-1492f11c043a";

    public override string Name => "list";

    public override string Description =>
        """
        Gets a list of firewall rules for a SQL server. This command retrieves all
        firewall rules configured for the specified SQL server, including their IP address ranges
        and rule names. Returns an array of firewall rule objects with their properties.
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var firewallRules = await _sqlService.ListFirewallRulesAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.RetryPolicy,
                cancellationToken);

            SetResult(context, new(firewallRules ?? []));
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

    public record FirewallRuleListResult(List<SqlServerFirewallRule> FirewallRules);
}
