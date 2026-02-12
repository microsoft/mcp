// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.Compute.Options;

public static class ComputeOptionDefinitions
{
    public const string VmNameName = "vm-name";
    public const string VmssNameName = "vmss-name";
    public const string InstanceIdName = "instance-id";
    public const string LocationName = "location";
    public const string DiskName = "disk";

    public static readonly Option<string> Disk = new($"--{DiskName}", "--name")
    {
        Description = "The name of the disk",
        Required = false
    };

    public static readonly Option<string> VmName = new($"--{VmNameName}", "--name")
    {
        Description = "The name of the virtual machine",
        Required = false
    };

    public static readonly Option<bool> InstanceView = new("--instance-view")
    {
        Description = "Include instance view details (only available when retrieving a specific VM)",
        Required = false
    };

    public static readonly Option<string> VmssName = new($"--{VmssNameName}", "--name")
    {
        Description = "The name of the virtual machine scale set",
        Required = false
    };

    public static readonly Option<string> InstanceId = new($"--{InstanceIdName}")
    {
        Description = "The instance ID of the virtual machine in the scale set",
        Required = false
    };

    public static readonly Option<string> Location = new($"--{LocationName}", "-l")
    {
        Description = "The Azure region/location",
        Required = true
    };
}
