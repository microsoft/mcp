// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Sql.Models;
using Azure.ResourceManager.Sql;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Services;

public class SqlService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<SqlService> logger) : BaseAzureService(tenantService), ISqlService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<SqlService> _logger = logger;

    public async Task<SqlDatabase?> GetDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup, cancellationToken);

            var sqlServerResource = await resourceGroupResource.Value
                .GetSqlServers()
                .GetAsync(serverName);

            var databaseResource = await sqlServerResource.Value
                .GetSqlDatabases()
                .GetAsync(databaseName);

            var database = databaseResource.Value.Data;

            return new SqlDatabase(
                Name: database.Name,
                Id: database.Id.ToString(),
                Type: database.ResourceType.ToString(),
                Location: database.Location.ToString(),
                Sku: database.Sku != null ? new DatabaseSku(
                    Name: database.Sku.Name,
                    Tier: database.Sku.Tier,
                    Capacity: database.Sku.Capacity,
                    Family: database.Sku.Family,
                    Size: database.Sku.Size
                ) : null,
                Status: database.Status?.ToString(),
                Collation: database.Collation,
                CreationDate: database.CreatedOn,
                MaxSizeBytes: database.MaxSizeBytes,
                ServiceLevelObjective: database.CurrentServiceObjectiveName,
                Edition: database.CurrentSku?.Name,
                ElasticPoolName: database.ElasticPoolId?.ToString().Split('/').LastOrDefault(),
                EarliestRestoreDate: database.EarliestRestoreOn,
                ReadScale: database.ReadScale?.ToString(),
                ZoneRedundant: database.IsZoneRedundant
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, databaseName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<List<SqlDatabase>> ListDatabasesAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup, cancellationToken);

            var sqlServerResource = await resourceGroupResource.Value
                .GetSqlServers()
                .GetAsync(serverName);

            var databases = new List<SqlDatabase>();

            await foreach (var databaseResource in sqlServerResource.Value.GetSqlDatabases().GetAllAsync())
            {
                var database = databaseResource.Data;

                databases.Add(new SqlDatabase(
                    Name: database.Name,
                    Id: database.Id.ToString(),
                    Type: database.ResourceType.ToString(),
                    Location: database.Location.ToString(),
                    Sku: database.Sku != null ? new DatabaseSku(
                        Name: database.Sku.Name,
                        Tier: database.Sku.Tier,
                        Capacity: database.Sku.Capacity,
                        Family: database.Sku.Family,
                        Size: database.Sku.Size
                    ) : null,
                    Status: database.Status?.ToString(),
                    Collation: database.Collation,
                    CreationDate: database.CreatedOn,
                    MaxSizeBytes: database.MaxSizeBytes,
                    ServiceLevelObjective: database.CurrentServiceObjectiveName,
                    Edition: database.CurrentSku?.Name,
                    ElasticPoolName: database.ElasticPoolId?.ToString().Split('/').LastOrDefault(),
                    EarliestRestoreDate: database.EarliestRestoreOn,
                    ReadScale: database.ReadScale?.ToString(),
                    ZoneRedundant: database.IsZoneRedundant
                ));
            }

            return databases;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL databases. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<List<SqlServerEntraAdministrator>> GetEntraAdministratorsAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup, cancellationToken);

            var sqlServerResource = await resourceGroupResource.Value
                .GetSqlServers()
                .GetAsync(serverName);

            var entraAdministrators = new List<SqlServerEntraAdministrator>();

            await foreach (var adminResource in sqlServerResource.Value.GetSqlServerAzureADAdministrators().GetAllAsync(cancellationToken))
            {
                var admin = adminResource.Data;
                entraAdministrators.Add(new SqlServerEntraAdministrator(
                    Name: admin.Name,
                    Id: admin.Id.ToString(),
                    Type: admin.ResourceType.ToString(),
                    AdministratorType: admin.AdministratorType?.ToString(),
                    Login: admin.Login,
                    Sid: admin.Sid?.ToString(),
                    TenantId: admin.TenantId?.ToString(),
                    AzureADOnlyAuthentication: admin.IsAzureADOnlyAuthenticationEnabled
                ));
            }

            return entraAdministrators;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL server Entra ID administrators. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<List<SqlElasticPool>> GetElasticPoolsAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup, cancellationToken);

            var sqlServerResource = await resourceGroupResource.Value
                .GetSqlServers()
                .GetAsync(serverName);

            var elasticPools = new List<SqlElasticPool>();

            await foreach (var poolResource in sqlServerResource.Value.GetElasticPools().GetAllAsync())
            {
                var pool = poolResource.Data;
                elasticPools.Add(new SqlElasticPool(
                    Name: pool.Name,
                    Id: pool.Id.ToString(),
                    Type: pool.ResourceType.ToString(),
                    Location: pool.Location.ToString(),
                    Sku: pool.Sku != null ? new ElasticPoolSku(
                        Name: pool.Sku.Name,
                        Tier: pool.Sku.Tier,
                        Capacity: pool.Sku.Capacity,
                        Family: pool.Sku.Family,
                        Size: pool.Sku.Size
                    ) : null,
                    State: pool.State?.ToString(),
                    CreationDate: pool.CreatedOn,
                    MaxSizeBytes: pool.MaxSizeBytes,
                    PerDatabaseSettings: pool.PerDatabaseSettings != null ? new ElasticPoolPerDatabaseSettings(
                        MinCapacity: pool.PerDatabaseSettings.MinCapacity,
                        MaxCapacity: pool.PerDatabaseSettings.MaxCapacity
                    ) : null,
                    ZoneRedundant: pool.IsZoneRedundant,
                    LicenseType: pool.LicenseType?.ToString(),
                    DatabaseDtuMin: null, // DTU properties not available in current SDK
                    DatabaseDtuMax: null,
                    Dtu: null,
                    StorageMB: null
                ));
            }

            return elasticPools;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL elastic pools. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<List<SqlServerFirewallRule>> ListFirewallRulesAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var subscriptionResource = await _subscriptionService.GetSubscription(subscription, null, retryPolicy);

            var resourceGroupResource = await subscriptionResource
                .GetResourceGroupAsync(resourceGroup, cancellationToken);

            var sqlServerResource = await resourceGroupResource.Value
                .GetSqlServers()
                .GetAsync(serverName);

            var firewallRules = new List<SqlServerFirewallRule>();

            await foreach (var firewallRuleResource in sqlServerResource.Value.GetSqlFirewallRules().GetAllAsync(cancellationToken))
            {
                var rule = firewallRuleResource.Data;
                firewallRules.Add(new SqlServerFirewallRule(
                    Name: rule.Name,
                    Id: rule.Id.ToString(),
                    Type: rule.ResourceType.ToString() ?? "Unknown",
                    StartIpAddress: rule.StartIPAddress,
                    EndIpAddress: rule.EndIPAddress
                ));
            }

            return firewallRules;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL server firewall rules. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }
}
