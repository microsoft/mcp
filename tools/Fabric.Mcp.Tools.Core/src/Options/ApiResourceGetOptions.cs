// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Fabric.Mcp.Tools.Core.Options;

public sealed class ApiResourceGetOptions()
{
    [Option(Description = "The REST API to call: fabric (https://api.fabric.microsoft.com) or powerbi (https://api.powerbi.com). Defaults to fabric. Selects both the host and the token scope; other hosts are not allowed.")]
    public string? Api { get; set; }

    [Option(Description = "The relative REST path, with an optional query string. Must start with /v1/ for fabric or /v1.0/myorg/ for powerbi, for example /v1/connections or /v1.0/myorg/gateways. Absolute URLs, fragments, whitespace, empty or dot segments, and encoded path separators are rejected.")]
    public required string Path { get; set; }

    [Option(Description = "Maximum JSON response size in bytes. Defaults to 1048576 (1 MiB); maximum 16777216 (16 MiB). Larger responses fail instead of being truncated.")]
    public int? MaxResponseBytes { get; set; }
}
