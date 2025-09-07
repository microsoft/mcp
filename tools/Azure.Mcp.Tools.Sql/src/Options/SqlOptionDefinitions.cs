// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options;

public static class SqlOptionDefinitions
{
    public const string ServerName = "server";
    public const string DatabaseName = "database";
    public const string FirewallRuleName = "firewall-rule-name";
    public const string StartIpAddress = "start-ip-address";
    public const string EndIpAddress = "end-ip-address";

    public static readonly Option<string> Server = new(
        $"--{ServerName}"
    )
    {
        Description = "The Azure SQL Server name.",
        Required = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}"
    )
    {
        Description = "The Azure SQL Database name.",
        Required = true
    };

    public static readonly Option<string> FirewallRuleNameOption = new(
        $"--{FirewallRuleName}"
    )
    {
        Description = "The name of the firewall rule.",
        Required = true
    };

    public static readonly Option<string> StartIpAddressOption = new(
        $"--{StartIpAddress}"
    )
    {
        Description = "The start IP address of the firewall rule range.",
        Required = true
    };

    public static readonly Option<string> EndIpAddressOption = new(
        $"--{EndIpAddress}"
    )
    {
        Description = "The end IP address of the firewall rule range.",
        Required = true
    };
}
