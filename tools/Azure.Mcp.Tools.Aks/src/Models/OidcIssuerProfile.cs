// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class OidcIssuerProfile
{
    public bool? Enabled { get; set; }
    [System.Text.Json.Serialization.JsonPropertyName("issuerURL")]
    public string? IssuerUrl { get; set; }
}
