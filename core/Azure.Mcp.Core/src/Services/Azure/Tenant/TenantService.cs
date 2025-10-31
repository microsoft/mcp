// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Caching;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.Tenant;

public class TenantService : BaseAzureService, ITenantService
{
    private readonly IAzureTokenCredentialProvider _credentialProvider;
    private readonly ICacheService _cacheService;
    private const string CacheGroup = "tenant";
    private const string CacheKey = "tenants";
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromHours(12);

    public TenantService(
        IAzureTokenCredentialProvider credentialProvider,
        ICacheService cacheService)
    {
        _credentialProvider = credentialProvider;
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        TenantService = this;
    }

    /// <inheritdoc/>
    public async Task<List<TenantResource>> GetTenants()
    {
        // Try to get from cache first
        var cachedResults = await _cacheService.GetAsync<List<TenantResource>>(CacheGroup, CacheKey, s_cacheDuration);
        if (cachedResults != null)
        {
            return cachedResults;
        }

        // If not in cache, fetch from Azure
        var results = new List<TenantResource>();

        var options = AddDefaultPolicies(new ArmClientOptions());
        var client = new ArmClient(await GetCredential(), default, options);

        await foreach (var tenant in client.GetTenants())
        {
            results.Add(tenant);
        }

        // Cache the results
        await _cacheService.SetAsync(CacheGroup, CacheKey, results, s_cacheDuration);
        return results;
    }

    /// <inheritdoc/>
    public bool IsTenantId(string tenantId)
    {
        return Guid.TryParse(tenantId, out _);
    }

    /// <inheritdoc/>
    public async Task<string> GetTenantId(string tenantIdOrName)
    {
        if (IsTenantId(tenantIdOrName))
        {
            return tenantIdOrName;
        }

        return await GetTenantIdByName(tenantIdOrName);
    }

    /// <inheritdoc/>
    public async Task<string> GetTenantIdByName(string tenantName)
    {
        var tenants = await GetTenants();
        var tenant = tenants.FirstOrDefault(t => t.Data.DisplayName?.Equals(tenantName, StringComparison.OrdinalIgnoreCase) == true) ??
            throw new Exception($"Could not find tenant with name {tenantName}");

        string? tenantId = tenant.Data.TenantId?.ToString();
        if (tenantId == null)
            throw new InvalidOperationException($"Tenant {tenantName} has a null TenantId");

        return tenantId.ToString();
    }

    /// <inheritdoc/>
    public async Task<string> GetTenantNameById(string tenantId)
    {
        var tenants = await GetTenants();
        var tenant = tenants.FirstOrDefault(t => t.Data.TenantId?.ToString().Equals(tenantId, StringComparison.OrdinalIgnoreCase) == true) ??
            throw new Exception($"Could not find tenant with ID {tenantId}");

        string? tenantName = tenant.Data.DisplayName;
        if (tenantName == null)
            throw new InvalidOperationException($"Tenant with ID {tenantId} has a null DisplayName");

        return tenantName;
    }

    /// <inheritdoc/>
    public async Task<TokenCredential> GetTokenCredentialAsync(string? tenantId, CancellationToken cancellationToken)
    {
        return await _credentialProvider.GetTokenCredentialAsync(tenantId, cancellationToken);
    }
}
