// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Tools.DeviceRegistry.Options;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.DeviceRegistry.Commands;

public abstract class BaseDeviceRegistryCommand<
    [DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] T>
    : SubscriptionCommand<T>
    where T : BaseDeviceRegistryOptions, new();
