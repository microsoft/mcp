// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FunctionApp.Models;

namespace Azure.Mcp.Tools.FunctionApp.Services;

public interface IFunctionAppService
{
    Task<List<FunctionAppInfo>?> ListFunctionApps(
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
