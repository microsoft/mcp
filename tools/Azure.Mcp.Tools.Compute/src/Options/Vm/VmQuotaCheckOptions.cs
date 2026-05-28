// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmQuotaCheckOptions : BaseComputeOptions
{
    public string? Location { get; set; }
    public string? FamilyPrefix { get; set; }
    public int? RequestedVCpus { get; set; }
}
