// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

public class FileShareSnapshot
{
    public string? Name { get; set; }

    public string? FileShareName { get; set; }

    public DateTime? CreatedTime { get; set; }

    public string? ProvisioningState { get; set; }

    public Dictionary<string, string>? Tags { get; set; }
}
