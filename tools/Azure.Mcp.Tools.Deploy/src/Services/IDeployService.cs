// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Deploy.Services;

public interface IDeployService
{
    Task<string> GetAzdResourceLogsAsync(
        string workspaceFolder,
        string azdEnvName,
        string subscriptionId,
        int? limit = null);
}
