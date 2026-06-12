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
        options.Agent = parseResult.GetValueOrDefault(SreAgentOptionDefinitions.Agent);
        return options;
    }

    protected static Task<string> ResolveEndpointAsync(
        ISreAgentService sreAgentService,
        TOptions options,
        CancellationToken cancellationToken)
        => SreAgentCommandHelpers.ResolveAgentEndpointAsync(sreAgentService, options, cancellationToken);
}
