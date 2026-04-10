// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Options;
using Azure.Mcp.Tools.Sql.Options.FirewallRule;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Sql.Commands.FirewallRule;

public sealed class FirewallRuleCreateCommand(ILogger<FirewallRuleCreateCommand> logger)
    : BaseSqlCommand<FirewallRuleCreateOptions>(logger)
{
    private const string CommandTitle = "Create SQL Server Firewall Rule";

    public override string Id => "37c43190-c3f5-4cd2-beda-3ecc2e3ec049";

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
        command.Validators.Add(commandResult =>
        {
            var startIp = commandResult.GetValueOrDefault(SqlOptionDefinitions.StartIpAddressOption);
            var endIp = commandResult.GetValueOrDefault(SqlOptionDefinitions.EndIpAddressOption);

            var startIpIsValid = !string.IsNullOrEmpty(startIp) && IsValidIpAddress(startIp);
            var endIpIsValid = !string.IsNullOrEmpty(endIp) && IsValidIpAddress(endIp);

            if (!startIpIsValid)
            {
                commandResult.AddError($"Invalid start IP address format: '{startIp}'. Must be a valid IPv4 address.");
            }

            if (!endIpIsValid)
            {
                commandResult.AddError($"Invalid end IP address format: '{endIp}'. Must be a valid IPv4 address.");
            }

            if (startIpIsValid && endIpIsValid && IsDangerousRange(startIp!, endIp!))
            {
                commandResult.AddError(
                    "The specified IP range is not allowed. A range of 0.0.0.0 to 0.0.0.0 enables access from all Azure services, and a range of 0.0.0.0 to 255.255.255.255 opens access to the entire internet. " +
                    "These overly permissive rules are blocked for security. Specify a narrower IP range instead."
                );
            }
        });
    }

    protected override FirewallRuleCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.FirewallRuleName = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.FirewallRuleNameOption.Name);
        options.StartIpAddress = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.StartIpAddressOption.Name);
        options.EndIpAddress = parseResult.GetValueOrDefault<string>(SqlOptionDefinitions.EndIpAddressOption.Name);
        return options;
    }

    // IP address must be a dotted-quad IPv4 format (e.g. 10.0.0.1).
    // The .ToString() check rejects non-canonical forms (e.g. 0, 4294967295) that would bypass the dangerous-range string checks via alternate representations.
    internal static bool IsValidIpAddress(string ipAddress) =>
        IPAddress.TryParse(ipAddress, out var parsed) && parsed.AddressFamily == AddressFamily.InterNetwork && parsed.ToString() == ipAddress;

    internal static bool IsDangerousRange(string startIp, string endIp)
    {
        // Block 0.0.0.0 - 0.0.0.0 (opens server to all Azure-internal traffic)
        if (startIp == "0.0.0.0" && endIp == "0.0.0.0")
        {
            return true;
        }

        // Block 0.0.0.0 - 255.255.255.255 (opens server to entire internet)
        if (startIp == "0.0.0.0" && endIp == "255.255.255.255")
        {
            return true;
        }

        return false;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
                options.RetryPolicy,
                cancellationToken);

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
