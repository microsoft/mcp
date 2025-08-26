// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Options;

namespace Azure.Mcp.Tools.Monitor.Commands;

public abstract class BaseMonitorCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : SubscriptionOptions, IWorkspaceOptions, new()
{
    private readonly Option<string> _workspaceOption = WorkspaceOptionDefinitions.Workspace;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        RequireResourceGroup();
        command.Options.Add(_workspaceOption);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Workspace = parseResult.GetValue(_workspaceOption);
        return options;
    }
}
