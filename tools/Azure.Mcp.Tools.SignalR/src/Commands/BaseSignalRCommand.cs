// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.SignalR.Options;

namespace Azure.Mcp.Tools.SignalR.Commands;

/// <summary>
/// Base command for all Azure SignalR commands.
/// </summary>
public abstract class BaseSignalRCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)]
TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseSignalROptions, new();
