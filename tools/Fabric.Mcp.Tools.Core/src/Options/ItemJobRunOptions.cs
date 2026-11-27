// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ItemJobRunOptions()
{
    [Option(Description = "The explicit ID of the Microsoft Fabric workspace containing the item. Must be a nonempty UUID.")]
    public required string WorkspaceId { get; set; }

    [Option(Description = "The explicit ID of the Microsoft Fabric item to execute. Must be a nonempty UUID.")]
    public required string ItemId { get; set; }

    [Option(Description = "The job type supported by this item. Required; no default or discovery. Encoded as one path component, with a tool budget of 1,024 UTF-8 bytes.")]
    public required string JobType { get; set; }

    [Option(Description = "Optional JSON object passed unchanged as executionData. Workload-specific content is opaque. Omit to omit the field. At most 1 MiB UTF-8 and 32 nesting levels; the serialized request also has a 1 MiB budget.")]
    public string? ExecutionData { get; set; }

    [Option(Description = "Optional JSON array passed as the API parameters field. Each object requires name, type, and value; names must be case-insensitively unique and at most 256 characters. Types and values remain workload-specific. Omit to omit the field; unsupported parameters are never stripped or retried. At most 1 MiB UTF-8 and 32 nesting levels; the serialized request also has a 1 MiB budget.")]
    public string? JobParameters { get; set; }
}
