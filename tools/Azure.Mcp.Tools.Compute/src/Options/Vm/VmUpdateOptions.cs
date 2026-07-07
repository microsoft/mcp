// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmUpdateOptions : BaseComputeOptions
{
    public string? VmName { get; set; }

    public string? VmSize { get; set; }

    public string? Tags { get; set; }

    public string? LicenseType { get; set; }

    public string? BootDiagnostics { get; set; }

    public string? UserData { get; set; }
}
