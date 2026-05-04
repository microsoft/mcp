// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Monitor.Commands.WebTests;

public abstract class BaseMonitorWebTestsCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions>
    where TOptions : SubscriptionOptions, new()
{
    protected BaseMonitorWebTestsCommand() : base()
    {
    }
}
