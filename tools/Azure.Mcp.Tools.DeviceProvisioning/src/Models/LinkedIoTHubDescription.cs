// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public record LinkedIoTHubDescription(
    string Name,
    string Location,
    bool? ApplyAllocationPolicy,
    int? AllocationWeight);

