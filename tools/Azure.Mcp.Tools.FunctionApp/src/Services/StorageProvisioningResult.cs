// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FunctionApp.Services;

/// <summary>
/// The storage account backing a Function App. <see cref="ConnectionString"/> is empty when managed identity is used.
/// </summary>
internal readonly record struct StorageProvisioningResult(string AccountName, string ConnectionString);
