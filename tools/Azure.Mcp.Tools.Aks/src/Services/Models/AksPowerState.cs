// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Aks.Services.Models;

internal sealed class AksPowerState
{
    /// <summary> Tells whether the cluster is Running or Stopped. </summary>
    public string? Code { get; set; }
}
