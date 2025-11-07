// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Services;

public interface IDeployService
{
    Task<string> GetResourceLogsAsync(
        string workspaceFolder,
        string subscriptionId,
        string resourceGroupName,
        int? limit = null);
}
