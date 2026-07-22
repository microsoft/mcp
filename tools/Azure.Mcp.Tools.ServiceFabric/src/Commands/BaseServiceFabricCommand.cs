// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.ServiceFabric.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ServiceFabric.Commands;

public abstract class BaseServiceFabricCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseServiceFabricOptions, new();
