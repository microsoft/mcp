// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.Mcp.Tools.IoTHub.Services;

internal sealed record ResolvedTarget(
    RoutingTargetDescriptor Descriptor,
    ResourceIdentifier ParentResourceId,
    ResourceIdentifier RoutedResourceId);
