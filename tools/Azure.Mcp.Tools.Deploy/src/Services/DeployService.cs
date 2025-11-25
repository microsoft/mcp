// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Deploy.Services.Util;

namespace Azure.Mcp.Tools.Deploy.Services;

public class DeployService(ITenantService tenantService) : BaseAzureService(tenantService), IDeployService
{
    public async Task<string> GetResourceLogsAsync(
         string subscriptionId,
         string resourceGroupName,
         int? limit = null,
         CancellationToken cancellationToken = default)
    {
        TokenCredential credential = await GetCredential(cancellationToken);
        string result = await AzdResourceLogService.GetAzdResourceLogsAsync(
            credential,
            subscriptionId,
            resourceGroupName,
            limit);
        return result;
    }
}
