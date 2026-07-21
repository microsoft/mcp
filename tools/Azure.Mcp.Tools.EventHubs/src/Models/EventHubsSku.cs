// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.EventHubs.Models;

public sealed record EventHubsSku(string? Name, string? Tier, int? Capacity);
