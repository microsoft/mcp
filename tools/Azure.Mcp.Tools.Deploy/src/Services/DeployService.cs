// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Azure.Monitor.Query.Logs;

namespace Azure.Mcp.Tools.Deploy.Services;

public class DeployService(IAzureService azureService) : BaseAzureService(azureService), IDeployService
{
    public async Task<string> GetAzdResourceLogsAsync(
         string workspaceFolder,
         string azdEnvName,
         string subscriptionId,
         int? limit = null,
         string? tenant = null,
         CancellationToken cancellationToken = default)
    {
        var armClient = await CreateArmClientAsync(tenant, cancellationToken: cancellationToken);
        var logsQueryClient = await CreateLogsQueryClientAsync(tenant, cancellationToken);

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

    private async Task<LogsQueryClient> CreateLogsQueryClientAsync(string? tenant, CancellationToken cancellationToken)
    {
        var credential = await GetCredential(tenant, cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Transport = new HttpClientTransport(AzureService.GetClient());
        return new(credential, options);
    }
}
