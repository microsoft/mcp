// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Validation;

/// <summary>Fixed input guidance without payload values, unknown property names, or parser details.</summary>
internal sealed class FabricItemInputException(string message) : Exception(message);
