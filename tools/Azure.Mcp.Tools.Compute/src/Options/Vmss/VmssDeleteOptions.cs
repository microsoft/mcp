// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vmss;

public class VmssDeleteOptions : BaseComputeOptions
{
    public string? VmssName { get; set; }

    public bool ForceDeletion { get; set; }
}
