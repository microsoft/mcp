// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Tables.Services;

public interface ITablesService
{
    Task<List<string>> ListTables(
        string account,
        bool isCosmosDb,
        string subscription,
        string? tenant = null,
        RetryPolicyOptions? retryPolicy = null);
}
