// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.VirtualDesktop.Options;
using Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;

public abstract class BaseHostPoolCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : BaseVirtualDesktopCommand<T>
    where T : BaseHostPoolOptions, new()
{

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(VirtualDesktopOptionDefinitions.HostPool);
        command.Options.Add(VirtualDesktopOptionDefinitions.HostPoolResourceIdOption);
        command.Validators.Add(commandResult =>
        {
            // Retrieve values once and infer presence from non-empty values
            commandResult.TryGetValue(VirtualDesktopOptionDefinitions.HostPool, out string? hostPoolName);
            commandResult.TryGetValue(VirtualDesktopOptionDefinitions.HostPoolResourceIdOption, out string? hostPoolResourceId);

            var hasHostPool = !string.IsNullOrWhiteSpace(hostPoolName);
            var hasHostPoolResourceId = !string.IsNullOrWhiteSpace(hostPoolResourceId);

            // Validate that either hostpool or hostpool-resource-id is provided, but not both
            if (!hasHostPool && !hasHostPoolResourceId)
            {
                commandResult.AddError("Either --hostpool or --hostpool-resource-id must be provided.");
            }

            if (hasHostPool && hasHostPoolResourceId)
            {
                commandResult.AddError("Cannot specify both --hostpool and --hostpool-resource-id. Use only one.");
            }
        });
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.HostPoolName = parseResult.GetValueOrDefault<string>(VirtualDesktopOptionDefinitions.HostPool.Name);
        options.HostPoolResourceId = parseResult.GetValueOrDefault<string>(VirtualDesktopOptionDefinitions.HostPoolResourceIdOption.Name);
        return options;
    }
}
