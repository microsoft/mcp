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
        string subscription,
        string? resourceGroup,
        string agentName,
        string? tenant,
        RetryPolicyOptions? retryPolicy,
        CancellationToken cancellationToken)
    {
        var agent = await sreAgentService.GetAgentAsync(
            subscription,
            resourceGroup,
            agentName,
            tenant,
            retryPolicy,
            cancellationToken);

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
        BaseSreAgentOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Agent);

        return ResolveAgentEndpointAsync(
            sreAgentService,
            options.Subscription,
            options.ResourceGroup,
            options.Agent,
            options.Tenant,
            options.RetryPolicy,
            cancellationToken);
    }

    /// <summary>
    /// Returns the resource group containing the agent, falling back to a Resource Graph lookup
    /// when the caller did not supply <c>--resource-group</c>. Used by ARM-based connector
    /// commands which need the RG as part of the resource path.
    /// </summary>
    public static async Task<string> ResolveAgentResourceGroupAsync(
        ISreAgentService sreAgentService,
        BaseSreAgentOptions options,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.ResourceGroup))
        {
            return options.ResourceGroup!;
        }

        return await sreAgentService.ResolveAgentResourceGroupAsync(
            options.Subscription!,
            options.Agent,
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

    internal static SreAgentThreadCreateRequest CreateThreadRequest(string message, string agentName)
    {
        var (userId, displayName) = GetUser();
        return new(new(message, userId, displayName, agentName));
    }

    internal static SreAgentThreadMessageRequest CreateMessageRequest(string message, string? agentName = null)
    {
        var (userId, displayName) = GetUser();
        return new(message, userId, displayName, agentName);
    }

    internal static (string UserId, string DisplayName) GetUser(string fallback = "mcp-user")
    {
        var user = Environment.GetEnvironmentVariable("USER") ?? Environment.GetEnvironmentVariable("USERNAME") ?? fallback;
        return (user, user == fallback ? "MCP User" : user);
    }
}
