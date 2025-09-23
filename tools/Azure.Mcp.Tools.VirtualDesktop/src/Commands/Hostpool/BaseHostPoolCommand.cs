// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.HostPoolName = parseResult.GetValueOrDefault<string>(VirtualDesktopOptionDefinitions.HostPool.Name);
        options.HostPoolResourceId = parseResult.GetValueOrDefault<string>(VirtualDesktopOptionDefinitions.HostPoolResourceIdOption.Name);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);
        if (!result.IsValid)
        {
            return result;
        }

        // Retrieve values once and infer presence from non-empty values
        commandResult.TryGetValue(VirtualDesktopOptionDefinitions.HostPool, out string? hostPoolName);
        commandResult.TryGetValue(VirtualDesktopOptionDefinitions.HostPoolResourceIdOption, out string? hostPoolResourceId);

        var hasHostPool = !string.IsNullOrWhiteSpace(hostPoolName);
        var hasHostPoolResourceId = !string.IsNullOrWhiteSpace(hostPoolResourceId);

        // Validate that either hostpool or hostpool-resource-id is provided, but not both
        if (!hasHostPool && !hasHostPoolResourceId)
        {
            result.IsValid = false;
            result.ErrorMessage = "Either --hostpool or --hostpool-resource-id must be provided.";
            if (commandResponse != null)
            {
                commandResponse.Status = HttpStatusCode.BadRequest;
                commandResponse.Message = result.ErrorMessage;
            }
            return result;
        }

        if (hasHostPool && hasHostPoolResourceId)
        {
            result.IsValid = false;
            result.ErrorMessage = "Cannot specify both --hostpool and --hostpool-resource-id. Use only one.";
            if (commandResponse != null)
            {
                commandResponse.Status = HttpStatusCode.BadRequest;
                commandResponse.Message = result.ErrorMessage;
            }
            return result;
        }

        return result;
    }
}
