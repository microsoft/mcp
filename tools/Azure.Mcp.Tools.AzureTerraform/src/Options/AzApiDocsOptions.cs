// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AzureTerraform.Options;

public sealed class AzApiDocsOptions
{
    [Option(Description = "The Azure resource type in ARM format (e.g., Microsoft.Compute/virtualMachines, Microsoft.Storage/storageAccounts).")]
    public required string ResourceType { get; set; }

    [Option(Description = "The API version to use for the resource schema. If omitted, the latest stable version is used.")]
    public string? ApiVersion { get; set; }
}
