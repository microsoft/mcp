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
}
