// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceRegistry.Options;

public static class DeviceRegistryOptionDefinitions
{
    public const string NamespaceName = "namespace";

    public static readonly Option<string> Namespace = new($"--{NamespaceName}")
    {
        Description = "The name of the Azure Device Registry namespace.",
        Required = false
    };
}
