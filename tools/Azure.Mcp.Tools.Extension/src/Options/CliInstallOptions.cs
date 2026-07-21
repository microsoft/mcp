// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public sealed class CliInstallOptions
{
    [Option(Description = "The type of CLI tool to use. Supported values are 'az' for Azure CLI, 'azd' for Azure Developer CLI, and 'func' for Azure Functions Core Tools CLI.")]
    public required string CliType { get; set; }
}
