// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.Policy.Options;

namespace Azure.Mcp.Tools.Policy.Commands;

/// <summary>
/// Base class for subscription-scoped policy commands.
/// 
/// This type centralizes the common constraint that all policy command option types
/// derive from <see cref="BasePolicyOptions" /> and have a public parameterless
/// constructor, and applies the trimming annotations required by the tools
/// command infrastructure.
/// 
/// New policy commands that operate at the subscription level should inherit from
/// this class instead of <see cref="SubscriptionCommand{TOptions}" /> to ensure
/// consistent behavior and linker-safety across the toolset.
/// </summary>
public abstract class BasePolicyCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BasePolicyOptions, new();
