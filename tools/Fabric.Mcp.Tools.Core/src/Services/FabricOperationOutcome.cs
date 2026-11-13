// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Core.Models;

namespace Fabric.Mcp.Tools.Core.Services;

internal sealed record FabricOperationOutcome<T>(T? Result, FabricOperationReceipt? Operation) where T : class;
