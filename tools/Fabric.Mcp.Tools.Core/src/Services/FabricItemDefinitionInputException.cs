// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Services;

/// <summary>Safe, input-specific guidance that never includes definition content or host paths.</summary>
public sealed class FabricItemDefinitionInputException(string message) : Exception(message);
