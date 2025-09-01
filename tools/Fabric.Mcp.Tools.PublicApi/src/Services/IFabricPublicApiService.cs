// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Fabric.Mcp.Tools.PublicApi.Models;

namespace Fabric.Mcp.Tools.PublicApi.Services;

public interface IFabricPublicApiService
{
    Task<IEnumerable<string>> ListFabricWorkloadsAsync();

    Task<FabricWorkloadPublicApi> ListFabricWorkloadPublicApis(string workload);

    Task<IDictionary<string, string>> GetExamplesAsync(string workloadType);
}
