// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;

namespace Fabric.Mcp.Tools.Core.Models;

/// <summary>A schedule snapshot with optional, extensible settings and potentially sensitive job inputs.</summary>
public sealed record FabricItemSchedule()
{
    public required Guid Id { get; init; }
    public required bool Enabled { get; init; }

    // Preserve the API's UTC text, including examples without an explicit offset.
    public string? CreatedDateTime { get; init; }
    public JsonElement? Configuration { get; init; }
    public JsonElement? Owner { get; init; }
    public JsonElement? ExecutionData { get; init; }
}
