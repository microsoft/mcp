// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record FabricOperationError(string? ErrorCode = null, Guid? RequestId = null);
