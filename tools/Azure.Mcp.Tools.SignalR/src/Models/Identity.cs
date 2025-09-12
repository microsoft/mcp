// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Identity;

namespace Azure.Mcp.Tools.SignalR.Models;

public class Identity
{
    public string? Type { get; set; }

    public ManagedIdentityInfo? ManagedIdentityInfo { get; set; }
}
