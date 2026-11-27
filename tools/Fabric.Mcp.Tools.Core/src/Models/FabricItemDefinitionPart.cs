// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A definition part using Fabric's logical path and payload representation.</summary>
public sealed record FabricItemDefinitionPart(string Path, string Payload, string PayloadType);
