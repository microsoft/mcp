// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Compute.Options;

namespace Azure.Mcp.Tools.Compute.Commands;

/// <summary>
/// Base command for all Compute commands.
/// </summary>
public abstract class BaseComputeCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseComputeOptions, new();
