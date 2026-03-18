// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Functions.Models;

/// <summary>
/// Represents the supported runtime versions for a language including
/// supported, preview, deprecated, and default versions.
/// </summary>
public sealed class RuntimeVersionInfo
{
    public required IReadOnlyList<string> Supported { get; init; }

    public IReadOnlyList<string>? Preview { get; init; }

    public IReadOnlyList<string>? Deprecated { get; init; }

    public required string Default { get; init; }

    public IReadOnlyList<string>? FrameworkSupported { get; init; }
}
