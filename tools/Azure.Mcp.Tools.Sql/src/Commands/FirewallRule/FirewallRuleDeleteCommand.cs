// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

public sealed class FirewallRuleDeleteCommand(ILogger<FirewallRuleDeleteCommand> logger)
    : BaseSqlCommand<FirewallRuleDeleteOptions>(logger)
{
    private const string CommandTitle = "Delete SQL Server Firewall Rule";

    public override string Name => "delete";

    public override string Description =>
        """
        Deletes a firewall rule from a SQL server. This operation removes the specified
        firewall rule, potentially restricting access for the IP addresses that were
        previously allowed by this rule. The operation is idempotent - if the rule
        doesn't exist, no error is returned.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.FirewallRuleNameOption);
    }

    protected override FirewallRuleDeleteOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FirewallRuleName = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.FirewallRuleNameOption.Name);
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

            var deleted = await sqlService.DeleteFirewallRuleAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.FirewallRuleName!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(
                new FirewallRuleDeleteResult(deleted, options.FirewallRuleName!),
                SqlJsonContext.Default.FirewallRuleDeleteResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Rule: {Rule}, Options: {@Options}",
                options.Server, options.ResourceGroup, options.FirewallRuleName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == 404 =>
            "SQL server or firewall rule not found. Verify the server name, rule name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == 403 =>
            $"Authorization failed deleting the firewall rule. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => argEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => reqEx.Status,
        ArgumentException => 400,
        _ => base.GetStatusCode(ex)
    };

    internal record FirewallRuleDeleteResult(bool Deleted, string RuleName);
}
