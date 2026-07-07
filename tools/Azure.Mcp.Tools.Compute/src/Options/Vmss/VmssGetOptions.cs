// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public class VmssGetOptions : BaseComputeOptions
{
    public string? VmssName { get; set; }
    public string? InstanceId { get; set; }
}
