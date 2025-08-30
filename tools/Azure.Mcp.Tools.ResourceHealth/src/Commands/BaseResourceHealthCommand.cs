// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.ResourceHealth.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.ResourceHealth.Commands;

public abstract class BaseResourceHealthCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseResourceHealthOptions, new()
{
}
