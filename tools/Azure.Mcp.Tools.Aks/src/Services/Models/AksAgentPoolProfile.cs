// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Tools.Aks.Commands;

namespace Azure.Mcp.Tools.Aks.Services.Models;

internal sealed class AksAgentPoolProfile
{
    /// <summary> Number of agents (VMs) to host docker containers. Allowed values must be in the range of 0 to 1000 (inclusive) for user pools and in the range of 1 to 1000 (inclusive) for system pools. The default value is 1. </summary>
    public int? Count { get; set; }
    /// <summary> VM size availability varies by region. If a node contains insufficient compute resources (memory, cpu, etc) pods might fail to run correctly. For more details on restricted VM sizes, see: https://docs.microsoft.com/azure/aks/quotas-skus-regions. </summary>
    public string? VmSize { get; set; }
}