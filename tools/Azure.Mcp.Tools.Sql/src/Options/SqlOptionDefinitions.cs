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
    public const string AdministratorLogin = "administrator-login";
    public const string AdministratorPassword = "administrator-password";
    public const string Location = "location";
    public const string Version = "version";
    public const string PublicNetworkAccess = "public-network-access";
    public const string Force = "force";

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

    public static readonly Option<string> AdministratorLoginOption = new(
        $"--{AdministratorLogin}",
        "The administrator login name for the SQL server."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> AdministratorPasswordOption = new(
        $"--{AdministratorPassword}",
        "The administrator password for the SQL server."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> LocationOption = new(
        $"--{Location}",
        "The Azure region location where the SQL server will be created."
    )
    {
        IsRequired = true
    };

    public static readonly Option<string> VersionOption = new(
        $"--{Version}",
        "The version of SQL Server to create (e.g., '12.0')."
    )
    {
        IsRequired = false
    };

    public static readonly Option<string> PublicNetworkAccessOption = new(
        $"--{PublicNetworkAccess}",
        "Whether public network access is enabled for the SQL server ('Enabled' or 'Disabled')."
    )
    {
        IsRequired = false
    };

    public static readonly Option<bool> ForceOption = new(
        $"--{Force}",
        "Force delete the server without confirmation prompts."
    )
    {
        IsRequired = false
    };
}
