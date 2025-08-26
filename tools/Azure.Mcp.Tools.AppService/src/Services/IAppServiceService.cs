// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using AzureMcp.AppService.Models;
using AzureMcp.Core.Options;

namespace AzureMcp.AppService.Services;

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
