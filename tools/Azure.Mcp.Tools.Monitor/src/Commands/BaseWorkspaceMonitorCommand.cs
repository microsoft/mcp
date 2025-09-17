// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Monitor.Options;

namespace Azure.Mcp.Tools.Monitor.Commands;

public abstract class BaseWorkspaceMonitorCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : BaseMonitorCommand<TOptions>
    where TOptions : SubscriptionOptions, IWorkspaceOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(WorkspaceOptionDefinitions.Workspace);
    }

    protected override TOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Workspace = parseResult.GetValueOrDefault<string>(WorkspaceOptionDefinitions.Workspace.Name);
        return options;
    }
}
