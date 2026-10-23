// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.IoTHub.Services;

internal sealed record RoutingTargetDescriptor(
    string EndpointType,
    string ResourceType,
    string ApiVersion);
