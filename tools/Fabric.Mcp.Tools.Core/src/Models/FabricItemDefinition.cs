// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>The parts and optional format of a Fabric item definition, without an API response envelope.</summary>
public sealed record FabricItemDefinition(IReadOnlyList<FabricItemDefinitionPart> Parts, string? Format = null);
