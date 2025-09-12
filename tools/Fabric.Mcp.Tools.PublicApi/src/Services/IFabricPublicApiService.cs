// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.PublicApi.Models;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public interface IFabricPublicApiService
{
    Task<IEnumerable<string>> ListWorkloadsAsync();

    Task<FabricWorkloadPublicApi> GetWorkloadPublicApis(string workloadType);

    Task<IDictionary<string, string>> GetWorkloadExamplesAsync(string workloadType);

    string GetWorkloadItemDefinition(string workloadType);

    IEnumerable<string> GetTopicBestPractices(string topic);
}
