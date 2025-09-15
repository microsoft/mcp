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
        RetryPolicyOptions? retryPolicy = null);

    Task<FunctionAppInfo> CreateFunctionApp(
        string subscription,
        string resourceGroup,
        string functionAppName,
        string location,
        string? planName = null,
        string? hostingKind = null,
        string? sku = null,
        string? runtime = null,
        string? runtimeVersion = null,
        string? os = null,
        string? storageAccountName = null,
        string? containerAppsEnvironmentName = null,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
