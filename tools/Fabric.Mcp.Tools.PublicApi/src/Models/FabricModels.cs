// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.PublicApi.Models;

public record FabricWorkloadPublicApi(
    string apiSpecification,
    string apiModelDefinitions,
    IEnumerable<string> exampleUrls);
