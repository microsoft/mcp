// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.Kusto.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Kusto.Commands;

public abstract class BaseTableCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions, TResult>(
    ISubscriptionResolver subscriptionResolver)
    : BaseDatabaseCommand<TOptions, TResult>(subscriptionResolver)
    where TOptions : class, ISubscriptionOption, ITableOption
{
}
