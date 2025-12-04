// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.DeviceRegistry.Options.Namespace;

public sealed class NamespaceCreateOptions : BaseDeviceRegistryOptions
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public Dictionary<string, string>? Tags { get; set; }
}
