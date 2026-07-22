// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AvmDocumentationOptions
{
    [Option(Description = AzureTerraformOptionDescriptions.ModuleName)]
    public required string ModuleName { get; set; }

    [Option(Description = "The version of the Azure Verified Module (e.g., 0.4.0).")]
    public required string ModuleVersion { get; set; }
}
