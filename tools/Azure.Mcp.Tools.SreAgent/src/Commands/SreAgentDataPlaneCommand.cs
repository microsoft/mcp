// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.SreAgent.Options;
using Azure.Mcp.Tools.SreAgent.Services;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;

namespace Azure.Mcp.Tools.SreAgent.Commands;

public abstract class SreAgentDataPlaneCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions> : BaseSreAgentCommand<TOptions>
    where TOptions : BaseSreAgentOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(SreAgentOptionDefinitions.Agent);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Agent = parseResult.GetValueOrDefault<string>(SreAgentOptionDefinitions.Agent.Name);
        return options;
    }

    protected static async Task<string> ResolveEndpointAsync(
        ISreAgentService sreAgentService,
        TOptions options,
        CancellationToken cancellationToken)
    {
        var agents = await sreAgentService.ListAgentsAsync(
            options.Subscription!,
            options.ResourceGroup,
            options.Tenant,
            options.RetryPolicy,
            cancellationToken);

        var agent = agents.FirstOrDefault(a => string.Equals(a.Name, options.Agent, StringComparison.OrdinalIgnoreCase));
        if (agent is null)
        {
            throw new InvalidOperationException($"SRE Agent resource '{options.Agent}' was not found in subscription '{options.Subscription}'.");
        }

        if (string.IsNullOrWhiteSpace(agent.Endpoint))
        {
            throw new InvalidOperationException($"SRE Agent resource '{options.Agent}' does not expose a data-plane endpoint.");
        }

        return agent.Endpoint;
    }
}
