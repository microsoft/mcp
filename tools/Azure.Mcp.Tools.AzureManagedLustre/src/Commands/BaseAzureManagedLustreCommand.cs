// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.AzureManagedLustre.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.AzureManagedLustre.Commands;

public abstract class BaseAzureManagedLustreCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<BaseAzureManagedLustreCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : BaseAzureManagedLustreOptions, new()
{
    // Currently no additional options beyond subscription + resource group
    protected readonly ILogger<BaseAzureManagedLustreCommand<TOptions>> _logger = logger;
}
