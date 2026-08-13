// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace McpToolEvaluator.Core.Models;

public record ToolArea
{
    public required string AssemblyName { get; init; }

    public required string ToolNamespace { get; init; }
}
