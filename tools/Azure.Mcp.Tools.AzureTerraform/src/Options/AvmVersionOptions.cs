// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AvmVersionOptions
{
    [Option(Description = AzureTerraformOptionDescriptions.ModuleName)]
    public required string ModuleName { get; set; }
}
