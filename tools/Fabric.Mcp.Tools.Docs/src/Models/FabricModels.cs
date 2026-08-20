// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Docs.Models;

public record FabricPublicApi(
    string apiSpecification,
    IDictionary<string, string> apiModelDefinitions);
