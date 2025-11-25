// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Models;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public interface IFabricPublicApiService
{
    Task<IEnumerable<string>> ListWorkloadsAsync(CancellationToken cancellationToken);

    Task<FabricWorkloadPublicApi> GetWorkloadPublicApis(string workloadType, CancellationToken cancellationToken);

    Task<IDictionary<string, string>> GetWorkloadExamplesAsync(string workloadType, CancellationToken cancellationToken);

    string GetWorkloadItemDefinition(string workloadType);

    IEnumerable<string> GetTopicBestPractices(string topic);
}
