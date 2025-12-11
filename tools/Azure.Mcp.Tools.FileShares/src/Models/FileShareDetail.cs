// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.FileShares.Models;

public class FileShareDetail
{
    public string? Name { get; set; }

    public string? Location { get; set; }

    public string? ResourceGroup { get; set; }

    public string? ProvisioningState { get; set; }

    public Dictionary<string, string>? Tags { get; set; }
}
