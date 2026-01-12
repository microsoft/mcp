// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.FileShares.Options;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FileShares.Commands;

/// <summary>
/// Base command class for all File Shares commands.
/// Provides common command infrastructure and option registration.
/// </summary>
public abstract class BaseFileSharesCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(
    ILogger logger,
    IFileSharesService fileSharesService)
    : SubscriptionCommand<TOptions> where TOptions : BaseFileSharesOptions, new()
{
    protected readonly ILogger _logger = logger;
    protected readonly IFileSharesService _fileSharesService = fileSharesService;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // Additional option registration can be added here for common File Shares options
    }
}
