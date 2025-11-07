// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Deploy.Services.Util;

namespace Azure.Mcp.Tools.Deploy.Services;

public class DeployService() : BaseAzureService, IDeployService
{
    public async Task<string> GetResourceLogsAsync(
         string workspaceFolder,
         string subscriptionId,
         string resourceGroupName,
         int? limit = null)
    {
        TokenCredential credential = await GetCredential();
        string result = await ResourceLogService.GetResourceLogsAsync(
            credential,
            workspaceFolder,
            subscriptionId,
            resourceGroupName,
            limit);
        return result;
    }
}
