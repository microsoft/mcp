// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Models;

public sealed class ServicePrincipalProfile
{
    public string? ClientId { get; set; }

    /// <summary>
    /// Secret associated with the service principal. Typically not returned by GET operations.
    /// </summary>
    public string? Secret { get; set; }
}
