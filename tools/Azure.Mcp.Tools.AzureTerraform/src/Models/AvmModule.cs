// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed record AvmModule(
    string ModuleName,
    string Description,
    string Source,
    string RepoUrl,
    string ModuleType);
