// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.SreAgent.Models;

/// <summary>
/// Lightweight projection of an Azure SRE Agent ARM resource returned by list/get operations.
/// </summary>
public sealed class SreAgentResource
{
    public string? Name { get; set; }

    public string? Id { get; set; }

    public string? Location { get; set; }

    public string? ResourceGroup { get; set; }

    public string? ProvisioningState { get; set; }

    public string? Endpoint { get; set; }
}
