// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemCreateCommandResult(FabricItem? Item, FabricOperationReceipt? Operation = null);
