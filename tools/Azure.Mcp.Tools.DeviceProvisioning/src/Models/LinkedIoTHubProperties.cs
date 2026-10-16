// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceProvisioning.Models;

public class LinkedIoTHubProperties
{
    public string? Name { get; set; }

    public string? Location { get; set; }

    public bool? ApplyAllocationPolicy { get; set; }

    public int? AllocationWeight { get; set; }
}
