// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.VirtualDesktop.Options.Hostpool;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.VirtualDesktop.Options.SessionHost;

public sealed class SessionHostUserSessionListOptions : BaseHostPoolOptions
{
    [Option(Description = "The name of the session host. This is the computer name of the virtual machine in the host pool.")]
    public required string Sessionhost { get; set; }
}
