// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.VirtualDesktop.Options;

public static class VirtualDesktopOptionDefinitions
{
    public const string HostPoolName = "hostpool";
    public const string HostPoolResourceId = "hostpool-resource-id";
    public const string SessionHostName = "sessionhost";
    public const string ResourceGroupName = "resource-group";

    public static readonly Option<string> HostPool = new(
        $"--{HostPoolName}"
    )
    {
        Description = "The name of the Azure Virtual Desktop host pool. This is the unique name you chose for your hostpool."

    };

    public static readonly Option<string> HostPoolResourceIdOption = new(
        $"--{HostPoolResourceId}"
    )
    {
        Description = "The Azure resource ID of the host pool. When provided, this will be used instead of searching by name."

    };


    public static readonly Option<string> SessionHost = new(
        $"--{SessionHostName}"
    )
    {
        Description = "The name of the session host. This is the computer name of the virtual machine in the host pool.",
        Required = true
    };

    public static readonly Option<string> ResourceGroup = new(
        $"--{ResourceGroupName}"
    )
    {
        Description = "The name of the Azure resource group. This is a logical container for Azure resources.",
    };
}
