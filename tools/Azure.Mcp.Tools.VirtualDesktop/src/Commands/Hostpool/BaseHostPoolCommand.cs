// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.VirtualDesktop.Options;
using Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;

public abstract class BaseHostPoolCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : BaseVirtualDesktopCommand<T>
    where T : BaseHostPoolOptions, new()
{
    protected readonly Option<string> _hostPoolOption = VirtualDesktopOptionDefinitions.HostPool;
    protected readonly Option<string> _hostPoolResourceIdOption = VirtualDesktopOptionDefinitions.HostPoolResourceIdOption;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_hostPoolOption);
        command.AddOption(_hostPoolResourceIdOption);
    }

    protected override T BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.HostPoolName = parseResult.GetValueForOption(_hostPoolOption);
        options.HostPoolResourceId = parseResult.GetValueForOption(_hostPoolResourceIdOption);
        return options;
    }

    public override ValidationResult Validate(CommandResult commandResult, CommandResponse? commandResponse = null)
    {
        var result = base.Validate(commandResult, commandResponse);
        if (!result.IsValid)
        {
            return result;
        }

        var hostPoolName = commandResult.GetValueForOption(_hostPoolOption);
        var hostPoolResourceId = commandResult.GetValueForOption(_hostPoolResourceIdOption);

        // Validate that either hostpool or hostpool-resource-id is provided, but not both
        if (string.IsNullOrEmpty(hostPoolName) && string.IsNullOrEmpty(hostPoolResourceId))
        {
            result.IsValid = false;
            result.ErrorMessage = "Either --hostpool or --hostpool-resource-id must be provided.";
            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }
            return result;
        }

        if (!string.IsNullOrEmpty(hostPoolName) && !string.IsNullOrEmpty(hostPoolResourceId))
        {
            result.IsValid = false;
            result.ErrorMessage = "Cannot specify both --hostpool and --hostpool-resource-id. Use only one.";
            if (commandResponse != null)
            {
                commandResponse.Status = 400;
                commandResponse.Message = result.ErrorMessage;
            }
            return result;
        }

        return result;
    }
}
