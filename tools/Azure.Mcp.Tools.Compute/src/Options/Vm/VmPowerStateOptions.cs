// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmPowerStateOptions : BaseComputeOptions
{
    public string? VmName { get; set; }

    public string? State { get; set; }

    public bool NoWait { get; set; }

    public bool SkipShutdown { get; set; }
}
