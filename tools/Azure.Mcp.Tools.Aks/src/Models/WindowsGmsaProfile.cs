// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class WindowsGmsaProfile
{
    public string? DnsServer { get; set; }
    public bool? Enabled { get; set; }
    public string? RootDomainName { get; set; }
}

