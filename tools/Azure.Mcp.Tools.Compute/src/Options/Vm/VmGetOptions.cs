// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmGetOptions : BaseComputeOptions
{
    public string? VmName { get; set; }

    public bool InstanceView { get; set; }
}
