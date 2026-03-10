// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.StorageSync.Options;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.StorageSync.Commands;

/// <summary>
/// Base command class for all Storage Sync commands.
/// Provides common command infrastructure and option registration.
/// </summary>
public abstract class BaseStorageSyncCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>
    (bool resourceGroupRequired = true, bool storageSyncServiceRequired = true)
    : SubscriptionCommand<TOptions> where TOptions : BaseStorageSyncOptions, new()
{
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(resourceGroupRequired 
            ? OptionDefinitions.Common.ResourceGroup.AsRequired() 
            : OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(storageSyncServiceRequired 
            ? StorageSyncOptionDefinitions.StorageSyncService.Name.AsRequired() 
            : StorageSyncOptionDefinitions.StorageSyncService.Name.AsOptional());
    }
}
