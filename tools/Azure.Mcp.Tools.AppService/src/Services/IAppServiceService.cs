// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AppService.Models;
using Microsoft.Mcp.Core.Options;

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

    Task<IDictionary<string, string>> GetAppSettingsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<string> UpdateAppSettingsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string settingName,
        string settingUpdateType,
        string? settingValue = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<DeploymentDetails>> GetDeploymentsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? deploymentId = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<List<DetectorDetails>> ListDetectorsAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);

    Task<DiagnosisResults> DiagnoseDetectorAsync(
        string subscription,
        string resourceGroup,
        string appName,
        string detectorName,
        DateTimeOffset? startTime = null,
        DateTimeOffset? endTime = null,
        string? interval = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null,
        CancellationToken cancellationToken = default);
}
