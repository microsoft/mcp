// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.ResourceManager.Resources;

namespace Azure.Mcp.Core.Services.Azure.Tenant;

public interface ITenantService
{
    Task<List<TenantResource>> GetTenants();
    Task<string?> GetTenantId(string tenant);
    Task<string?> GetTenantIdByName(string tenantName);
    Task<string?> GetTenantNameById(string tenantId);
    bool IsTenantId(string tenant);
}
