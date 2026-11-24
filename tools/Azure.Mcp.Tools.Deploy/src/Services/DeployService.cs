// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core.Pipeline;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Deploy.Services.Util;
using Azure.Monitor.Query.Logs;
using Microsoft.Mcp.Core.Services.Host;

namespace Azure.Mcp.Tools.Deploy.Services;

public class DeployService(IAzureService azureService, IFileSystem fileSystem) : BaseAzureService(azureService), IDeployService
{
    public async Task<string> GetAzdResourceLogsAsync(
         string workspaceFolder,
         string azdEnvName,
         string subscriptionId,
         int? limit = null,
         CancellationToken cancellationToken = default)
    {
        fileSystem.EnsureAccess();
        var armClient = await CreateArmClientAsync(cancellationToken: cancellationToken);
        var logsQueryClient = await CreateLogsQueryClientAsync(cancellationToken);

        string result = await AzdResourceLogService.GetAzdResourceLogsAsync(
            fileSystem,
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
        var credential = await GetCredential(null, cancellationToken);
        var options = AddDefaultPolicies(new LogsQueryClientOptions());
        options.Transport = new HttpClientTransport(AzureService.GetClient());
        return new(credential, options);
    }
}
