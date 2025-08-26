// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppService.Services;

public interface IAppServiceService
{
    Task<DatabaseConnectionInfo> AddDatabaseAsync(
        string appName,
        string resourceGroup,
        string databaseType,
        string databaseServer,
        string databaseName,
        string connectionString,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
