// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>
/// Maps a logical definition part to a file relative to the selected source directory.
/// When SourcePath is omitted, PartPath is also the relative source path.
/// </summary>
public sealed record FabricItemDefinitionFile(string PartPath, string? SourcePath = null);
