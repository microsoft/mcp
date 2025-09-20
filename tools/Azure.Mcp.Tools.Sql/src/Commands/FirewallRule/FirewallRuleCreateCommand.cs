// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

public sealed class FirewallRuleCreateCommand(ILogger<FirewallRuleCreateCommand> logger)
    : BaseSqlCommand<FirewallRuleCreateOptions>(logger)
{
    private const string CommandTitle = "Create SQL Server Firewall Rule";

    public override string Name => "create";

    public override string Description =>
        """
        Creates a firewall rule for a SQL server. Firewall rules control which IP addresses
        are allowed to connect to the SQL server. You can specify either a single IP address
        (by setting start and end IP to the same value) or a range of IP addresses. Returns
        the created firewall rule with its properties.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        Idempotent = false,
        OpenWorld = false,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SqlOptionDefinitions.FirewallRuleNameOption);
        command.Options.Add(SqlOptionDefinitions.StartIpAddressOption);
        command.Options.Add(SqlOptionDefinitions.EndIpAddressOption);
    }

    protected override FirewallRuleCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FirewallRuleName = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.FirewallRuleNameOption.Name);
        options.StartIpAddress = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.StartIpAddressOption.Name);
        options.EndIpAddress = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.EndIpAddressOption.Name);
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

            var firewallRule = await sqlService.CreateFirewallRuleAsync(
                options.Server!,
                options.ResourceGroup!,
                options.Subscription!,
                options.FirewallRuleName!,
                options.StartIpAddress!,
                options.EndIpAddress!,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(firewallRule), SqlJsonContext.Default.FirewallRuleCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Rule: {Rule}, Options: {@Options}",
                options.Server, options.ResourceGroup, options.FirewallRuleName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.NotFound =>
            "SQL server not found. Verify the server name, resource group, and that you have access.",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Forbidden =>
            $"Authorization failed creating the firewall rule. Verify you have appropriate permissions. Details: {reqEx.Message}",
        RequestFailedException reqEx when reqEx.Status == (int)HttpStatusCode.Conflict =>
            "A firewall rule with this name already exists. Choose a different name or update the existing rule.",
        RequestFailedException reqEx => reqEx.Message,
        ArgumentException argEx => $"Invalid IP address format: {argEx.Message}",
        _ => base.GetErrorMessage(ex)
    };

    protected override HttpStatusCode GetStatusCode(Exception ex) => ex switch
    {
        RequestFailedException reqEx => (HttpStatusCode)reqEx.Status,
        ArgumentException => HttpStatusCode.BadRequest,
        _ => base.GetStatusCode(ex)
    };

    internal record FirewallRuleCreateResult(SqlServerFirewallRule FirewallRule);
}
