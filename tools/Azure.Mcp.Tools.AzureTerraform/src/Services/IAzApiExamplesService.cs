// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.AzureTerraform.Models;

namespace Azure.Mcp.Tools.AzureTerraform.Services;

public interface IAzApiExamplesService
{
    Task<List<AzApiExample>> GetExamplesAsync(string resourceTypeName, CancellationToken cancellationToken = default);
}
