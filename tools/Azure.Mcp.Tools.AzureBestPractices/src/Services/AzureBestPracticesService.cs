// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;

namespace Azure.Mcp.Tools.AzureBestPractices.Services;

public sealed class AzureBestPracticesService : IAzureBestPracticesService
{
    public Task<string> GetCodingAgentBestPracticesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var assembly = typeof(AzureBestPracticesService).Assembly;
        var resource = EmbeddedResourceHelper.FindEmbeddedResource(assembly, "azure-coding-agent-best-practices.txt");
        return Task.FromResult(EmbeddedResourceHelper.ReadEmbeddedResource(assembly, resource));
    }
}
