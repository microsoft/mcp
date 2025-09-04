// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Models;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public interface IFabricPublicApiService
{
    Task<IEnumerable<string>> ListFabricWorkloadsAsync();

    Task<FabricWorkloadPublicApi> GetFabricWorkloadPublicApis(string workloadType);

    Task<IDictionary<string, string>> GetExamplesAsync(string workloadType);

    string GetFabricWorkloadItemDefinition(string workloadType);

    IEnumerable<string> GetTopicBestPractices(string topic);
}
