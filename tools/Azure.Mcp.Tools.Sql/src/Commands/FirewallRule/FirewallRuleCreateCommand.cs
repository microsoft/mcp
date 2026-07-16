// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
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
    Id = "37c43190-c3f5-4cd2-beda-3ecc2e3ec049",
    Name = "create",
    Title = "Create SQL Server Firewall Rule",
    Description = """
        Creates a firewall rule for a SQL server. Firewall rules control which IP addresses
        are allowed to connect to the SQL server. You can specify either a single IP address
        (by setting start and end IP to the same value) or a range of IP addresses. Returns
        the created firewall rule with its properties.
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class FirewallRuleCreateCommand(ISqlService sqlService, ILogger<FirewallRuleCreateCommand> logger, ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<FirewallRuleCreateOptions, FirewallRuleCreateCommand.FirewallRuleCreateResult>(subscriptionResolver)
{
    private readonly ISqlService _sqlService = sqlService;
    private readonly ILogger<FirewallRuleCreateCommand> _logger = logger;

    public override void ValidateOptions(FirewallRuleCreateOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        var startIpIsValid = !string.IsNullOrEmpty(options.StartIpAddress) && IsValidIpAddress(options.StartIpAddress);
        var endIpIsValid = !string.IsNullOrEmpty(options.EndIpAddress) && IsValidIpAddress(options.EndIpAddress);

        if (!startIpIsValid)
        {
            validationResult.Errors.Add($"Invalid start IP address format: '{options.StartIpAddress}'. Must be a valid IPv4 address.");
        }

        if (!endIpIsValid)
        {
            validationResult.Errors.Add($"Invalid end IP address format: '{options.EndIpAddress}'. Must be a valid IPv4 address.");
        }

        if (startIpIsValid && endIpIsValid && IsDangerousRange(options.StartIpAddress, options.EndIpAddress))
        {
            validationResult.Errors.Add(
                "The specified IP range is not allowed. A range of 0.0.0.0 to 0.0.0.0 enables access from all Azure services, and a range of 0.0.0.0 to 255.255.255.255 opens access to the entire internet. " +
                "These overly permissive rules are blocked for security. Specify a narrower IP range instead."
            );
        }
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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, FirewallRuleCreateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var firewallRule = await _sqlService.CreateFirewallRuleAsync(
                options.Server,
                options.ResourceGroup,
                options.Subscription!,
                options.FirewallRuleName,
                options.StartIpAddress,
                options.EndIpAddress,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(firewallRule), SqlJsonContext.Default.FirewallRuleCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Rule: {Rule}.",
                options.Server, options.ResourceGroup, options.FirewallRuleName);
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

    public sealed record FirewallRuleCreateResult(SqlServerFirewallRule FirewallRule);
}
