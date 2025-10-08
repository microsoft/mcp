// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Table.Services;

public interface ITableService
{
    Task<List<string>> ListTables(
        string account,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
