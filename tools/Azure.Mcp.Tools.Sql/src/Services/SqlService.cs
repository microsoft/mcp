// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services.Models;
using Azure.ResourceManager.Sql;
using Azure.ResourceManager.Sql.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Sql.Services;

public class SqlService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<SqlService> logger) : BaseAzureResourceService(subscriptionService, tenantService), ISqlService
{
    private readonly ILogger<SqlService> _logger = logger;

    /// <summary>
    /// Retrieves a specific SQL database from an Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server hosting the database</param>
    /// <param name="databaseName">The name of the database to retrieve</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>The SQL database if found, otherwise throws KeyNotFoundException</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database is not found</exception>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<SqlDatabase> GetDatabaseAsync(
        string serverName,
        string databaseName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ExecuteSingleResourceQueryAsync(
                "Microsoft.Sql/servers/databases",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToSqlDatabaseModel,
                $"name =~ '{EscapeKqlString(databaseName)}'",
                cancellationToken);

            if (result == null)
            {
                throw new KeyNotFoundException($"SQL database '{databaseName}' not found in resource group '{resourceGroup}' for subscription '{subscription}'.");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL database. Server: {Server}, Database: {Database}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, databaseName, resourceGroup, subscription);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of all SQL databases from an Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server to list databases from</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>A list of SQL databases on the specified server</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<List<SqlDatabase>> ListDatabasesAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.Sql/servers/databases",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToSqlDatabaseModel,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error listing SQL databases. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of Microsoft Entra ID (formerly Azure AD) administrators for an Azure SQL Server.
    /// These administrators can authenticate to the SQL server using their Entra ID credentials.
    /// </summary>
    /// <param name="serverName">The name of the SQL server to get administrators for</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>A list of Entra ID administrators configured for the SQL server</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<List<SqlServerEntraAdministrator>> GetEntraAdministratorsAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.Sql/servers/administrators",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToSqlServerEntraAdministratorModel,
                $"id contains '/servers/{EscapeKqlString(serverName)}/'",
                50,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL server Entra ID administrators. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of elastic pools from an Azure SQL Server.
    /// Elastic pools provide a cost-effective solution for managing multiple databases with varying usage patterns.
    /// </summary>
    /// <param name="serverName">The name of the SQL server to get elastic pools from</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>A list of elastic pools configured on the SQL server</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<List<SqlElasticPool>> GetElasticPoolsAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.Sql/servers/elasticPools",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToSqlElasticPoolModel,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL elastic pools. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a list of firewall rules configured for an Azure SQL Server.
    /// Firewall rules control which IP addresses are allowed to connect to the SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server to get firewall rules for</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>A list of firewall rules configured on the SQL server</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<List<SqlServerFirewallRule>> ListFirewallRulesAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ExecuteResourceQueryAsync(
                "Microsoft.Sql/servers/firewallRules",
                resourceGroup,
                subscription,
                retryPolicy,
                ConvertToSqlFirewallRuleModel,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL server firewall rules. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    /// <summary>
    /// Creates a firewall rule for an Azure SQL Server.
    /// Firewall rules control which IP addresses are allowed to connect to the SQL server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server to create firewall rule for</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="firewallRuleName">The name of the firewall rule to create</param>
    /// <param name="startIpAddress">The start IP address of the firewall rule range</param>
    /// <param name="endIpAddress">The end IP address of the firewall rule range</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>The created firewall rule</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<SqlServerFirewallRule> CreateFirewallRuleAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        string firewallRuleName,
        string startIpAddress,
        string endIpAddress,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(serverName, resourceGroup, subscription, firewallRuleName, startIpAddress, endIpAddress);

        try
        {
            // Use ARM client directly for create operations
            var armClient = await CreateArmClientAsync(null, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            var sqlServerResource = await resourceGroupResource.Value.GetSqlServers().GetAsync(serverName);

            var firewallRuleData = new ResourceManager.Sql.SqlFirewallRuleData()
            {
                StartIPAddress = startIpAddress,
                EndIPAddress = endIpAddress
            };

            var operation = await sqlServerResource.Value.GetSqlFirewallRules().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                firewallRuleName,
                firewallRuleData,
                cancellationToken);

            var firewallRule = operation.Value;

            return new SqlServerFirewallRule(
                Name: firewallRule.Data.Name,
                Id: firewallRule.Data.Id.ToString(),
                Type: firewallRule.Data.ResourceType?.ToString() ?? "Unknown",
                StartIpAddress: firewallRule.Data.StartIPAddress,
                EndIpAddress: firewallRule.Data.EndIPAddress
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Rule: {Rule}",
                serverName, resourceGroup, subscription, firewallRuleName);
            throw;
        }
    }

    /// <summary>
    /// Deletes a firewall rule from an Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="firewallRuleName">The name of the firewall rule to delete</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>True if the firewall rule was successfully deleted</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<bool> DeleteFirewallRuleAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        string firewallRuleName,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(serverName, resourceGroup, subscription, firewallRuleName);

        try
        {
            // Use ARM client directly for delete operations
            var armClient = await CreateArmClientAsync(null, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);
            var sqlServerResource = await resourceGroupResource.Value.GetSqlServers().GetAsync(serverName);

            var firewallRuleResource = await sqlServerResource.Value.GetSqlFirewallRules().GetAsync(firewallRuleName);

            await firewallRuleResource.Value.DeleteAsync(Azure.WaitUntil.Completed, cancellationToken);

            _logger.LogInformation(
                "Successfully deleted SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Rule: {Rule}",
                serverName, resourceGroup, firewallRuleName);

            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            _logger.LogWarning(
                "Firewall rule not found during delete operation. Server: {Server}, ResourceGroup: {ResourceGroup}, Rule: {Rule}",
                serverName, resourceGroup, firewallRuleName);

            // Return false to indicate the rule was not found (idempotent delete)
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL server firewall rule. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Rule: {Rule}",
                serverName, resourceGroup, subscription, firewallRuleName);
            throw;
        }
    }

    /// <summary>
    /// Creates a new Azure SQL Server.
    /// </summary>
    /// <param name="serverName">The name of the SQL server to create</param>
    /// <param name="resourceGroup">The name of the resource group</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="location">The Azure region location where the SQL server will be created</param>
    /// <param name="administratorLogin">The administrator login name for the SQL server</param>
    /// <param name="administratorPassword">The administrator password for the SQL server</param>
    /// <param name="version">The version of SQL Server to create (optional, defaults to latest)</param>
    /// <param name="publicNetworkAccess">Whether public network access is enabled (optional)</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>The created SQL server</returns>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<SqlServer> CreateServerAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        string location,
        string administratorLogin,
        string administratorPassword,
        string? version,
        string? publicNetworkAccess,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(serverName, resourceGroup, subscription, location, administratorLogin, administratorPassword);

        try
        {
            // Use ARM client directly for create operations  
            var armClient = await CreateArmClientAsync(null, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            var serverData = new SqlServerData(location)
            {
                AdministratorLogin = administratorLogin,
                AdministratorLoginPassword = administratorPassword,
                Version = version ?? "12.0", // Default to SQL Server 2014 (12.0)
            };

            // Set PublicNetworkAccess if specified
            if (!string.IsNullOrEmpty(publicNetworkAccess))
            {
                // Set the public network access value - defaults to "Enabled" if not "Disabled"
                serverData.PublicNetworkAccess = publicNetworkAccess.Equals("Enabled", StringComparison.OrdinalIgnoreCase)
                    ? ServerNetworkAccessFlag.Enabled
                    : ServerNetworkAccessFlag.Disabled;
            }

            var operation = await resourceGroupResource.Value.GetSqlServers().CreateOrUpdateAsync(
                Azure.WaitUntil.Completed,
                serverName,
                serverData,
                cancellationToken);

            var server = operation.Value;
            var tags = server.Data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>();

            return new SqlServer(
                Name: server.Data.Name,
                FullyQualifiedDomainName: server.Data.FullyQualifiedDomainName,
                Location: server.Data.Location.ToString(),
                ResourceGroup: resourceGroup,
                Subscription: subscription,
                AdministratorLogin: server.Data.AdministratorLogin,
                Version: server.Data.Version,
                State: server.Data.State?.ToString(),
                PublicNetworkAccess: server.Data.PublicNetworkAccess?.ToString(),
                Tags: tags
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error creating SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}, Location: {Location}",
                serverName, resourceGroup, subscription, location);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a specific SQL server from Azure.
    /// </summary>
    /// <param name="serverName">The name of the SQL server</param>
    /// <param name="resourceGroup">The name of the resource group containing the server</param>
    /// <param name="subscription">The subscription ID or name</param>
    /// <param name="retryPolicy">Optional retry policy configuration for resilient operations</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests</param>
    /// <returns>The SQL server if found, otherwise throws KeyNotFoundException</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified server is not found</exception>
    /// <exception cref="ArgumentException">Thrown when required parameters are null or empty</exception>
    public async Task<SqlServer> GetServerAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(serverName, resourceGroup, subscription);

        try
        {
            // Use ARM client directly for get operations  
            var armClient = await CreateArmClientAsync(null, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            var serverResource = await resourceGroupResource.Value.GetSqlServers().GetAsync(serverName);
            var server = serverResource.Value;
            var tags = server.Data.Tags?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, string>();

            return new SqlServer(
                Name: server.Data.Name,
                FullyQualifiedDomainName: server.Data.FullyQualifiedDomainName,
                Location: server.Data.Location.ToString(),
                ResourceGroup: resourceGroup,
                Subscription: subscription,
                AdministratorLogin: server.Data.AdministratorLogin,
                Version: server.Data.Version,
                State: server.Data.State?.ToString(),
                PublicNetworkAccess: server.Data.PublicNetworkAccess?.ToString(),
                Tags: tags
            );
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == 404)
        {
            throw new KeyNotFoundException($"SQL server '{serverName}' not found in resource group '{resourceGroup}' for subscription '{subscription}'.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error getting SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    public async Task<bool> DeleteServerAsync(
        string serverName,
        string resourceGroup,
        string subscription,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters(serverName, resourceGroup, subscription);

        try
        {
            // Use ARM client directly for delete operations  
            var armClient = await CreateArmClientAsync(null, retryPolicy);
            var subscriptionResource = armClient.GetSubscriptionResource(Azure.ResourceManager.Resources.SubscriptionResource.CreateResourceIdentifier(subscription));
            var resourceGroupResource = await subscriptionResource.GetResourceGroupAsync(resourceGroup);

            var serverResource = await resourceGroupResource.Value.GetSqlServers().GetAsync(serverName);

            var operation = await serverResource.Value.DeleteAsync(
                Azure.WaitUntil.Completed,
                cancellationToken);

            return true;
        }
        catch (Azure.RequestFailedException reqEx) when (reqEx.Status == 404)
        {
            _logger.LogWarning(
                "SQL server not found during delete operation. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            return false; // Server doesn't exist
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting SQL server. Server: {Server}, ResourceGroup: {ResourceGroup}, Subscription: {Subscription}",
                serverName, resourceGroup, subscription);
            throw;
        }
    }

    private static SqlDatabase ConvertToSqlDatabaseModel(JsonElement item)
    {
        Models.SqlDatabaseData? sqlDatabase = Azure.Mcp.Tools.Sql.Services.Models.SqlDatabaseData.FromJson(item);
        if (sqlDatabase == null)
            throw new InvalidOperationException("Failed to parse SQL database data");

        return new SqlDatabase(
                Name: sqlDatabase.ResourceName ?? "Unknown",
                Id: sqlDatabase.ResourceId ?? "Unknown",
                Type: sqlDatabase.ResourceType ?? "Unknown",
                Location: sqlDatabase.Location,
                Sku: sqlDatabase.Sku != null ? new DatabaseSku(
                    Name: sqlDatabase.Sku.Name,
                    Tier: sqlDatabase.Sku.Tier,
                    Capacity: sqlDatabase.Sku.Capacity,
                    Family: sqlDatabase.Sku.Family,
                    Size: sqlDatabase.Sku.Size
                ) : null,
                Status: sqlDatabase.Properties?.Status,
                Collation: sqlDatabase.Properties?.Collation,
                CreationDate: sqlDatabase.Properties?.CreatedOn,
                MaxSizeBytes: sqlDatabase.Properties?.MaxSizeBytes,
                ServiceLevelObjective: sqlDatabase.Properties?.CurrentServiceObjectiveName,
                Edition: sqlDatabase.Properties?.CurrentSku?.Name,
                ElasticPoolName: sqlDatabase.Properties?.ElasticPoolId?.ToString().Split('/').LastOrDefault(),
                EarliestRestoreDate: sqlDatabase.Properties?.EarliestRestoreOn,
                ReadScale: sqlDatabase.Properties?.ReadScale,
                ZoneRedundant: sqlDatabase.Properties?.IsZoneRedundant
            );
    }

    private static SqlServerEntraAdministrator ConvertToSqlServerEntraAdministratorModel(JsonElement item)
    {
        SqlServerAadAdministratorData? admin = SqlServerAadAdministratorData.FromJson(item);
        if (admin == null)
            throw new InvalidOperationException("Failed to parse SQL server AAD administrator data");

        return new SqlServerEntraAdministrator(
                    Name: admin.ResourceName ?? "Unknown",
                    Id: admin.ResourceId ?? "Unknown",
                    Type: admin.ResourceType ?? "Unknown",
                    AdministratorType: admin.Properties?.AdministratorType,
                    Login: admin.Properties?.Login,
                    Sid: admin.Properties?.Sid?.ToString(),
                    TenantId: admin.Properties?.TenantId?.ToString(),
                    AzureADOnlyAuthentication: admin.Properties?.IsAzureADOnlyAuthenticationEnabled
                );
    }

    private static SqlElasticPool ConvertToSqlElasticPoolModel(JsonElement item)
    {
        SqlElasticPoolData? elasticPool = SqlElasticPoolData.FromJson(item);
        if (elasticPool == null)
            throw new InvalidOperationException("Failed to parse SQL elastic pool data");

        return new SqlElasticPool(
                    Name: elasticPool.ResourceName ?? "Unknown",
                    Id: elasticPool.ResourceId ?? "Unknown",
                    Type: elasticPool.ResourceType ?? "Unknown",
                    Location: elasticPool.Location,
                    Sku: elasticPool.Sku != null ? new ElasticPoolSku(
                        Name: elasticPool.Sku.Name,
                        Tier: elasticPool.Sku.Tier,
                        Capacity: elasticPool.Sku.Capacity,
                        Family: elasticPool.Sku.Family,
                        Size: elasticPool.Sku.Size
                    ) : null,
                    State: elasticPool.Properties?.State,
                    CreationDate: elasticPool.Properties?.CreatedOn,
                    MaxSizeBytes: elasticPool.Properties?.MaxSizeBytes,
                    PerDatabaseSettings: elasticPool.Properties?.PerDatabaseSettings != null ? new Sql.Models.ElasticPoolPerDatabaseSettings(
                        MinCapacity: elasticPool.Properties.PerDatabaseSettings.MinCapacity,
                        MaxCapacity: elasticPool.Properties.PerDatabaseSettings.MaxCapacity
                    ) : null,
                    ZoneRedundant: elasticPool.Properties?.IsZoneRedundant,
                    LicenseType: elasticPool.Properties?.LicenseType,
                    DatabaseDtuMin: null, // DTU properties not available in current SDK
                    DatabaseDtuMax: null,
                    Dtu: null,
                    StorageMB: null
                );
    }

    private static SqlServerFirewallRule ConvertToSqlFirewallRuleModel(JsonElement item)
    {
        Models.SqlFirewallRuleData? firewallRule = Azure.Mcp.Tools.Sql.Services.Models.SqlFirewallRuleData.FromJson(item);
        if (firewallRule == null)
            throw new InvalidOperationException("Failed to parse SQL firewall rule data");

        return new SqlServerFirewallRule(
            Name: firewallRule.ResourceName ?? "Unknown",
            Id: firewallRule.ResourceId ?? "Unknown",
            Type: firewallRule.ResourceType ?? "Unknown",
            StartIpAddress: firewallRule.Properties?.StartIPAddress,
            EndIpAddress: firewallRule.Properties?.EndIPAddress
        );
    }
}
