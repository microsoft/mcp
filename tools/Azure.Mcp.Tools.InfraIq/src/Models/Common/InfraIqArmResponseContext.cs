// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.InfraIq.Models.Common;

public sealed record InfraIqArmResponseContext(
    string? RequestId,
    string? ClientRequestId,
    DateTimeOffset? Date,
    string? RetryAfter);
