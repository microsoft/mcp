// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemDeleteCommandResult(Guid WorkspaceId, Guid ItemId, bool HardDeleteRequested);
