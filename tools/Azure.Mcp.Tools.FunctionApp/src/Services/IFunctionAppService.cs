// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FunctionApp.Models;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public interface IFunctionAppService
{
    Task<List<FunctionAppInfo>?> GetFunctionApp(
        string subscription,
        string? functionAppName,
        string? resourceGroup,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<FunctionAppInfo> CreateFunctionApp(
        string subscription,
        string resourceGroup,
        string functionApp,
        string location,
        string? appServicePlan = null,
        string? planType = null,
        string? planSku = null,
        string? runtime = null,
        string? runtimeVersion = null,
        string? operatingSystem = null,
        string? storageAccount = null,
        string? storageAuthMode = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);

    Task<FunctionAppInfo> CreateContainerAppFunctionApp(
        string subscription,
        string resourceGroup,
        string functionApp,
        string location,
        string? runtime = null,
        string? runtimeVersion = null,
        string? storageAccount = null,
        string? storageAuthMode = null,
        string? containerAppsEnvironment = null,
        string? tenant = null,
        CancellationToken cancellationToken = default);
}
