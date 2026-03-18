// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmDeleteOptions : BaseComputeOptions
{
    public string? VmName { get; set; }

    public bool ForceDeletion { get; set; }
}
