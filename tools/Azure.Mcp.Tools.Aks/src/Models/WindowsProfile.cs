// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class WindowsProfile
{
    public string? AdminPassword { get; set; }
    public string? AdminUsername { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("enableCSIProxy")]
    public bool? EnableCsiProxy { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName("gmsaProfile")]
    public WindowsGmsaProfile? GmsaProfile { get; set; }

    public string? LicenseType { get; set; }
}
