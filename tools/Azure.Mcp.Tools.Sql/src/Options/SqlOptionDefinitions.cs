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
        $"--{ServerName}",
        "The Azure SQL Server name."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> Database = new(
        $"--{DatabaseName}",
        "The Azure SQL Database name."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> FirewallRuleNameOption = new(
        $"--{FirewallRuleName}",
        "The name of the firewall rule."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> StartIpAddressOption = new(
        $"--{StartIpAddress}",
        "The start IP address of the firewall rule range."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> EndIpAddressOption = new(
        $"--{EndIpAddress}",
        "The end IP address of the firewall rule range."
    )
    {
        IsRequired = true
    };
}
