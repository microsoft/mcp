// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.AppService.Models;

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
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<WebappDetails>> GetWebAppsAsync(
        string subscription,
        string? resourceGroup = null,
        string? appName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<WebappDiagnosticCategoryDetails>> GetWebAppDiagnosticCategoriesAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? diagnosticCategory = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<WebappDetectorDetails>> GetWebAppDetectorsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string diagnosticCategory,
        string? detectorName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<WebappAnalysisDetails>> GetWebAppAnalysesAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string diagnosticCategory,
        string? analysisName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
