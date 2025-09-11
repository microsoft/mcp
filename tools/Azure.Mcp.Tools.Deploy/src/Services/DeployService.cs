// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Deploy.Services.Util;

namespace Azure.Mcp.Tools.Deploy.Services;

public class DeployService() : BaseAzureService, IDeployService
{

    public async Task<string> GetAzdResourceLogsAsync(
         string workspaceFolder,
         string azdEnvName,
         string subscriptionId,
         int? limit = null)
    {
        TokenCredential credential = await GetCredential();
        string result = await AzdResourceLogService.GetAzdResourceLogsAsync(
            credential,
            workspaceFolder,
            azdEnvName,
            subscriptionId,
            limit);
        return result;
    }
}
