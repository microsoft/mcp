// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Fabric.Mcp.Tools.Core.Models;

public sealed record ItemScheduleGetCommandResult(
    Guid WorkspaceId,
    Guid ItemId,
    string JobType,
    FabricItemSchedule Schedule,
    long? RetryAfterSeconds = null,
    DateTimeOffset? RetryAfterUtc = null);
