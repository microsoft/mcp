// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmImageListOptions : BaseComputeOptions
{
    public string? Location { get; set; }
    public string? Alias { get; set; }
    public string? Publisher { get; set; }
    public string? Offer { get; set; }
    public string? Sku { get; set; }
    public int? Top { get; set; }
}
