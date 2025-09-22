// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Sql.Models;

namespace Azure.Mcp.Tools.Sql.Services;

public interface ISqlService
{
    /// <summary>
    /// Gets a SQL database from Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="databaseName">The name of the database</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The SQL database information</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the database is not found</exception>
    Task<SqlDatabase> GetDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new SQL database in Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="databaseName">The name of the database to create</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="skuName">Optional SKU name for the database</param>
    /// <param name="skuTier">Optional SKU tier for the database</param>
    /// <param name="skuCapacity">Optional SKU capacity for the database</param>
    /// <param name="collation">Optional collation for the database</param>
    /// <param name="maxSizeBytes">Optional maximum size in bytes for the database</param>
    /// <param name="elasticPoolName">Optional elastic pool name to assign the database to</param>
    /// <param name="zoneRedundant">Optional zone redundancy setting</param>
    /// <param name="readScale">Optional read scale setting</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created SQL database information</returns>
    Task<SqlDatabase> CreateDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        string? skuName = null,
        string? skuTier = null,
        int? skuCapacity = null,
        string? collation = null,
        long? maxSizeBytes = null,
        string? elasticPoolName = null,
        bool? zoneRedundant = null,
        string? readScale = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates configuration settings for an existing SQL database.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="databaseName">The name of the database to update</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="skuName">Optional SKU name for the database</param>
    /// <param name="skuTier">Optional SKU tier for the database</param>
    /// <param name="skuCapacity">Optional SKU capacity for the database</param>
    /// <param name="collation">Optional collation for the database</param>
    /// <param name="maxSizeBytes">Optional maximum size in bytes for the database</param>
    /// <param name="elasticPoolName">Optional elastic pool name to assign the database to</param>
    /// <param name="zoneRedundant">Optional zone redundancy setting</param>
    /// <param name="readScale">Optional read scale setting</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated SQL database information</returns>
    Task<SqlDatabase> UpdateDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        string? skuName = null,
        string? skuTier = null,
        int? skuCapacity = null,
        string? collation = null,
        long? maxSizeBytes = null,
        string? elasticPoolName = null,
        bool? zoneRedundant = null,
        string? readScale = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of databases for a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of SQL databases</returns>
    Task<List<SqlDatabase>> ListDatabasesAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of Microsoft Entra ID administrators for a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of SQL server Entra administrators</returns>
    Task<List<SqlServerEntraAdministrator>> GetEntraAdministratorsAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of elastic pools for a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of SQL elastic pools</returns>
    Task<List<SqlElasticPool>> GetElasticPoolsAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a list of firewall rules for a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of SQL server firewall rules</returns>
    Task<List<SqlServerFirewallRule>> ListFirewallRulesAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a firewall rule for a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="firewallRuleName">The name of the firewall rule</param>
    /// <param name="startIpAddress">The start IP address of the firewall rule range</param>
    /// <param name="endIpAddress">The end IP address of the firewall rule range</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created SQL server firewall rule</returns>
    Task<SqlServerFirewallRule> CreateFirewallRuleAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        string firewallRuleName,
        string startIpAddress,
        string endIpAddress,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a firewall rule from a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="firewallRuleName">The name of the firewall rule to delete</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the firewall rule was successfully deleted</returns>
    Task<bool> DeleteFirewallRuleAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        string firewallRuleName,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a SQL database from Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="databaseName">The name of the database to delete</param>
    /// <param name="resourceGroup">The resource group name</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the database was successfully deleted</returns>
    Task<bool> DeleteDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="location">The Azure region location where the SQL server will be created</param>
    /// <param name="administratorLogin">The administrator login name for the SQL server</param>
    /// <param name="administratorPassword">The administrator password for the SQL server</param>
    /// <param name="version">The version of SQL Server to create (optional, defaults to latest)</param>
    /// <param name="publicNetworkAccess">Whether public network access is enabled (optional)</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created SQL server information</returns>
    Task<SqlServer> CreateServerAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        string location,
        string administratorLogin,
        string administratorPassword,
        string? version,
        string? publicNetworkAccess,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The SQL server information</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the server is not found</exception>
    Task<SqlServer> GetServerAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists SQL servers in a resource group.
    /// </summary>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of SQL servers</returns>
    Task<List<SqlServer>> ListServersAsync(
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the server was successfully deleted</returns>
    Task<bool> DeleteServerAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default);
}
