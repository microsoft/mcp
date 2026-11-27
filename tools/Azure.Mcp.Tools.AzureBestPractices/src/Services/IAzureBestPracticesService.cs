// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureBestPractices.Services;

public interface IAzureBestPracticesService
{
    Task<string> GetCodingAgentBestPracticesAsync(CancellationToken cancellationToken = default);
}
