// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Extension.Options;

public sealed class CliGenerateOptions
{
    [Option(Description = "The user intent of the task to be solved by using the CLI tool. This user intent will be used to generate the appropriate CLI command to accomplish the desirable goal.")]
    public required string Intent { get; set; }

    [Option(Description = "The type of CLI tool to use. Supported values are 'az' for Azure CLI.")]
    public required string CliType { get; set; }
}
