// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.VirtualDesktop.Commands;

/// <summary>
/// Base command for all Virtual Desktop commands
/// </summary>
public abstract class BaseVirtualDesktopCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : SubscriptionOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        UseResourceGroup();
    }
}
