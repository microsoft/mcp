// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>Local packaging budgets, not Fabric API quotas or MCP tool options.</summary>
public sealed record FabricItemDefinitionFileLimits(
    int MaxParts = 1024,
    int MaxPartBytes = 16 * 1024 * 1024,
    int MaxTotalBytes = 32 * 1024 * 1024)
{
    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxParts);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaxPartBytes);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaxTotalBytes, MaxPartBytes);
    }
}
