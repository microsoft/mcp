// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.StorageSync.Options;

namespace Azure.Mcp.Tools.StorageSync.Commands;

/// <summary>
/// Base command class for all Storage Sync commands.
/// Provides common command infrastructure and option registration.
/// </summary>
public abstract class BaseStorageSyncCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    : SubscriptionCommand<TOptions> where TOptions : BaseStorageSyncOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        // Additional option registration can be added here for common Storage Sync options
    }
}
