// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.EventHubs.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.EventHubs.Commands;

public abstract class BaseEventHubsCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseEventHubsOptions, new();
