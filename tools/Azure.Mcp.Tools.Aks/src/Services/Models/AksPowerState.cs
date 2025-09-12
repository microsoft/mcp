// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands;

namespace Azure.Mcp.Tools.Aks.Services.Models;

internal sealed class AksPowerState
{
    /// <summary> Tells whether the cluster is Running or Stopped. </summary>
    public string? Code { get; set; }
}