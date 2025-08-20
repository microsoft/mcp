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
    /// <returns>The SQL database information, or null if not found</returns>
    Task<SqlDatabase?> GetDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
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
}
