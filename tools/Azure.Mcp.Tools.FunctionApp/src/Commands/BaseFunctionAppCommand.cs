// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FunctionApp.Commands;

public abstract class BaseFunctionAppCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions,
    TResult>
    : SubscriptionCommand<TOptions, TResult> where TOptions : SubscriptionOptions, new();
