// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Extensions.Logging;
using Azure.Mcp.Tools.Aks.Options;

namespace Azure.Mcp.Tools.Aks.Commands;

public abstract class BaseAksCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseAksOptions, new();
