// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemDefinitionGetCommandResult(
    FabricItemDefinition? Definition,
    FabricOperationReceipt? Operation = null,
    string? OutputDirectory = null);
