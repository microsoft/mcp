// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Logs;
using Azure.ResourceManager;

namespace Azure.Mcp.Tools.Deploy.Services;

public class DeployService(ITenantService tenantService) : BaseAzureService(tenantService), IDeployService
{
    public async Task<string> GetAzdResourceLogsAsync(
         string workspaceFolder,
         string azdEnvName,
         string subscriptionId,
         int? limit = null,
         CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(cancellationToken: cancellationToken);
        var logsQueryClient = await CreateLogsQueryClientAsync(cancellationToken);

        string result = await AzdResourceLogService.GetAzdResourceLogsAsync(
            armClient,
            logsQueryClient,
            workspaceFolder,
            azdEnvName,
            subscriptionId,
            limit,
            cancellationToken);
        return result;
    }

    private async Task<LogsQueryClient> CreateLogsQueryClientAsync(CancellationToken cancellationToken)
    {
        var credential = await GetCredential(cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Transport = new HttpClientTransport(TenantService.GetClient());
        return new LogsQueryClient(credential, options);
    }
}
