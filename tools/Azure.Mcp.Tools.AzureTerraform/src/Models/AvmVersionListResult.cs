// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureTerraform.Models;

public sealed record AvmVersionListResult(string ModuleName, List<AvmVersion> Versions);
