// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Data.Tables;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;

namespace Azure.Mcp.Tools.Tables.Services;

public class TablesService(ITenantService tenantService) : BaseAzureService(tenantService), ITablesService
{
    protected async Task<TableServiceClient> CreateTableServiceClient(
        string? account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        var options = ConfigureRetryPolicy(AddDefaultPolicies(new TableClientOptions()), retryPolicy);
        var defaultUri = $"https://{account}.table.core.windows.net";
        return new TableServiceClient(new Uri(defaultUri), await GetCredential(tenant, cancellationToken), options);
    }

    public async Task<List<string>> ListTables(
        string account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default)
    {
        ValidateRequiredParameters((nameof(account), account), (nameof(subscription), subscription));

        var tables = new List<string>();

        try
        {
            // First attempt with requested auth method
            var tableServiceClient = await CreateTableServiceClient(
                account,
                subscription,
                tenant,
                retryPolicy,
                cancellationToken);

            await foreach (var table in tableServiceClient.QueryAsync(cancellationToken: cancellationToken))
            {
                tables.Add(table.Name);
            }
            return tables;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error listing tables: {ex.Message}", ex);
        }
    }
}
