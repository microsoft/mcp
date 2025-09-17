// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.VirtualDesktop.Commands.Hostpool;
using Azure.Mcp.Tools.VirtualDesktop.Options;
using Azure.Mcp.Tools.VirtualDesktop.Options.SessionHost;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands.SessionHost;

public abstract class BaseSessionHostCommand
    : BaseHostPoolCommand<SessionHostUserSessionListOptions>
{

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(VirtualDesktopOptionDefinitions.SessionHost);
    }

    protected override SessionHostUserSessionListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.SessionHostName = parseResult.GetValueOrDefault<string>(VirtualDesktopOptionDefinitions.SessionHost.Name);
        return options;
    }
}
