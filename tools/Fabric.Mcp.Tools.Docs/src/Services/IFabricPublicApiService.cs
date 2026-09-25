// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Models;

namespace Fabric.Mcp.Tools.Docs.Services;

public interface IFabricPublicApiService
{
    Task<IEnumerable<string>> ListItemTypesAsync(CancellationToken cancellationToken);

    Task<FabricPublicApi> GetPublicApis(string itemType, CancellationToken cancellationToken);

    Task<IDictionary<string, string>> GetExamplesAsync(string itemType, CancellationToken cancellationToken);

    string GetItemDefinition(string itemType);

    IEnumerable<string> GetTopicBestPractices(string topic);
}
