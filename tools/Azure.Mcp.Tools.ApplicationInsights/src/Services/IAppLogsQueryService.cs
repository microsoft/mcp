// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.ApplicationInsights.Services;

public interface IAppLogsQueryService
{
    Task<IAppLogsQueryClient> CreateClientAsync(ResourceIdentifier resolvedResource, string? tenant = null, RetryPolicyOptions? retryPolicy = null);
}
