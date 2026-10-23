// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Sql.Options;

internal static class SqlOptionDescriptions
{
    internal const string Server = "The Azure SQL Server name.";
    internal const string Database = "The Azure SQL Database name.";
    internal const string FirewallRuleName = "The name of the firewall rule.";
    internal const string SkuName = "The SKU name for the database (e.g., Basic, S0, P1, GP_Gen5_2).";
    internal const string SkuTier = "The SKU tier for the database (e.g., Basic, Standard, Premium, GeneralPurpose).";
    internal const string SkuCapacity = "The SKU capacity (DTU or vCore count) for the database.";
    internal const string Collation = "The collation for the database (e.g., SQL_Latin1_General_CP1_CI_AS).";
    internal const string MaxSizeBytes = "The maximum size of the database in bytes.";
    internal const string ElasticPoolName = "The name of the elastic pool to assign the database to.";
    internal const string ZoneRedundant = "Whether the database should be zone redundant.";
    internal const string ReadScale = "Read scale option for the database (Enabled or Disabled).";
}
