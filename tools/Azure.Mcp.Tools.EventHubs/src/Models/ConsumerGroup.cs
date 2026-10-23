// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Models;

public sealed record ConsumerGroup(
    string Name,
    string Id,
    string ResourceGroup,
    string Namespace,
    string EventHub,
    string? Location = null,
    string? UserMetadata = null,
    DateTimeOffset? CreationTime = null,
    DateTimeOffset? UpdatedTime = null);
