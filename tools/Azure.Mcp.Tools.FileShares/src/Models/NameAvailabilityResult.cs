// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

public class NameAvailabilityResult
{
    public bool Available { get; set; }

    public string? Message { get; set; }

    public string? Reason { get; set; }
}
