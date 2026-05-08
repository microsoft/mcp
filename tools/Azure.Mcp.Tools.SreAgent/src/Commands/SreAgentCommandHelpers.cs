// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.SreAgent.Models;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.SreAgent.Commands;

internal static class SreAgentCommandHelpers
{
    public static async Task<string> ResolveAgentEndpointAsync(
        ISreAgentService sreAgentService,
        BaseSreAgentOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Agent);

        var agents = await sreAgentService.ListAgentsAsync(
            options.Subscription,
            options.ResourceGroup,
            options.Tenant,
            options.RetryPolicy,
            cancellationToken);

        var agent = agents.FirstOrDefault(a => string.Equals(a.Name, options.Agent, StringComparison.OrdinalIgnoreCase));
        if (agent is null)
        {
            throw new ArgumentException($"SRE Agent '{options.Agent}' was not found in subscription '{options.Subscription}'.");
        }

        if (string.IsNullOrWhiteSpace(agent.Endpoint))
        {
            throw new ArgumentException($"SRE Agent '{options.Agent}' does not expose a data-plane endpoint.");
        }

        return agent.Endpoint;
    }

    public static Dictionary<string, object>? ParseJsonObject(string? json, string optionName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var value = JsonSerializer.Deserialize(json, SreAgentJsonContext.Default.DictionaryStringObject)
            ?? throw new ArgumentException($"The --{optionName} value must be a JSON object.");
        return value;
    }

    public static Dictionary<string, string>? ParseJsonStringMap(string? json, string optionName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        var value = JsonSerializer.Deserialize(json, SreAgentJsonContext.Default.DictionaryStringString)
            ?? throw new ArgumentException($"The --{optionName} value must be a JSON object with string values.");
        return value;
    }
}

