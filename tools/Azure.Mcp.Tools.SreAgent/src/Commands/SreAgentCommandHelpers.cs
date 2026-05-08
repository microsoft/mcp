// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Commands;

internal static class SreAgentCommandHelpers
{
    public static async Task<string> ResolveAgentEndpointAsync(
        ISreAgentService sreAgentService,
        string subscription,
        string? resourceGroup,
        string agentName,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var agents = await sreAgentService.ListAgentsAsync(
            subscription,
            resourceGroup,
            tenant,
            retryPolicy,
            cancellationToken);

        var agent = agents.FirstOrDefault(a => string.Equals(a.Name, agentName, StringComparison.OrdinalIgnoreCase));
        if (agent is null)
        {
            throw new InvalidOperationException($"SRE Agent resource '{agentName}' was not found in the selected subscription and resource group.");
        }

        if (string.IsNullOrWhiteSpace(agent.Endpoint))
        {
            throw new InvalidOperationException($"SRE Agent resource '{agentName}' does not expose a data-plane endpoint.");
        }

        return agent.Endpoint;
    }
}
