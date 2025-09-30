// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options;

public static class SqlOptionDefinitions
{
    public const string ServerName = "server";
    public const string DatabaseName = "database";
    public const string NewDatabaseName = "new-database-name";
    public const string FirewallRuleName = "firewall-rule-name";
    public const string StartIpAddress = "start-ip-address";
    public const string EndIpAddress = "end-ip-address";
    public const string AdministratorLogin = "administrator-login";
    public const string AdministratorPassword = "administrator-password";
    public const string Location = "location";
    public const string Version = "version";
    public const string PublicNetworkAccess = "public-network-access";
    public const string Force = "force";
    public const string SkuName = "sku-name";
    public const string SkuTier = "sku-tier";
    public const string SkuCapacity = "sku-capacity";
    public const string Collation = "collation";
    public const string MaxSizeBytes = "max-size-bytes";
    public const string ElasticPoolName = "elastic-pool-name";
    public const string ZoneRedundant = "zone-redundant";
    public const string ReadScale = "read-scale";

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

    public static readonly Option<string> NewDatabaseNameOption = new(
        $"--{NewDatabaseName}"
    )
    {
        Description = "The new name for the Azure SQL Database.",
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
        $"--{AdministratorLogin}"
    )
    {
        Description = "The administrator login name for the SQL server.",
        Required = true
    };

    public static readonly Option<string> AdministratorPasswordOption = new(
        $"--{AdministratorPassword}"
    )
    {
        Description = "The administrator password for the SQL server.",
        Required = true
    };

    public static readonly Option<string> LocationOption = new(
        $"--{Location}"
    )
    {
        Description = "The Azure region location where the SQL server will be created.",
        Required = true
    };

    public static readonly Option<string> VersionOption = new(
        $"--{Version}"
    )
    {
        Description = "The version of SQL Server to create (e.g., '12.0').",
        Required = false
    };

    public static readonly Option<string> PublicNetworkAccessOption = new(
        $"--{PublicNetworkAccess}"
    )
    {
        Description = "Whether public network access is enabled for the SQL server ('Enabled' or 'Disabled').",
        Required = false
    };

    public static readonly Option<bool> ForceOption = new(
        $"--{Force}"
    )
    {
        Description = "Force delete the server without confirmation prompts.",
        Required = false
    };

    public static readonly Option<string> SkuNameOption = new(
        $"--{SkuName}"
    )
    {
        Description = "The SKU name for the database (e.g., Basic, S0, P1, GP_Gen5_2).",
        Required = false
    };

    public static readonly Option<string> SkuTierOption = new(
        $"--{SkuTier}"
    )
    {
        Description = "The SKU tier for the database (e.g., Basic, Standard, Premium, GeneralPurpose).",
        Required = false
    };

    public static readonly Option<int> SkuCapacityOption = new(
        $"--{SkuCapacity}"
    )
    {
        Description = "The SKU capacity (DTU or vCore count) for the database.",
        Required = false
    };

    public static readonly Option<string> CollationOption = new(
        $"--{Collation}"
    )
    {
        Description = "The collation for the database (e.g., SQL_Latin1_General_CP1_CI_AS).",
        Required = false
    };

    public static readonly Option<long> MaxSizeBytesOption = new(
        $"--{MaxSizeBytes}"
    )
    {
        Description = "The maximum size of the database in bytes.",
        Required = false
    };

    public static readonly Option<string> ElasticPoolNameOption = new(
        $"--{ElasticPoolName}"
    )
    {
        Description = "The name of the elastic pool to assign the database to.",
        Required = false
    };

    public static readonly Option<bool> ZoneRedundantOption = new(
        $"--{ZoneRedundant}"
    )
    {
        Description = "Whether the database should be zone redundant.",
        Required = false
    };

    public static readonly Option<string> ReadScaleOption = new(
        $"--{ReadScale}"
    )
    {
        Description = "Read scale option for the database (Enabled or Disabled).",
        Required = false
    };
}
