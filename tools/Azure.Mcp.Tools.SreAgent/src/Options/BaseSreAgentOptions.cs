// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Options;

/// <summary>
/// Interface for SRE Agent options that need agent endpoint resolution.
/// </summary>
public interface ISreAgentOption : ISubscriptionOption
{
    string? Agent { get; set; }
    string? ResourceGroup { get; set; }
    string? Tenant { get; set; }
    RetryPolicyOptions? RetryPolicy { get; set; }
}
