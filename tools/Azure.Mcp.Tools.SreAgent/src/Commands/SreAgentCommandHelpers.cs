// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tools.SreAgent.Options;
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

    public static Task<string> ResolveAgentEndpointAsync(
        ISreAgentService sreAgentService,
        ISreAgentOption options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Agent);

        return ResolveAgentEndpointAsync(
            sreAgentService,
            options.Subscription!,
            options.ResourceGroup,
            options.Agent!,
            options.Tenant,
            options.RetryPolicy,
            cancellationToken);
    }

    public static async Task<string> ResolveAgentResourceGroupAsync(
        ISreAgentService sreAgentService,
        ISreAgentOption options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Agent);

        if (!string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            return options.ResourceGroup!;
        }

        return await sreAgentService.ResolveAgentResourceGroupAsync(
            options.Subscription!,
            options.Agent!,
            options.Tenant,
            options.RetryPolicy,
            cancellationToken);
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
