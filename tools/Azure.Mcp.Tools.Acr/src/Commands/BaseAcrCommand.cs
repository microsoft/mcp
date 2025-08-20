// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Tools.Acr.Options;
using Azure.Mcp.Core.Commands.Subscription;

namespace Azure.Mcp.Tools.Acr.Commands;

public abstract class BaseAcrCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseAcrOptions, new();
