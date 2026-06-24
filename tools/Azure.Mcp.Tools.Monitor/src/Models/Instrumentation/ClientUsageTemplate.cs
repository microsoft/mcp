// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Monitor.Models.Instrumentation;

public sealed record ClientUsageTemplate
{
    public required string DirectUsage { get; init; }
    public required List<ClientUsageEntryTemplate> Usages { get; init; }
}
