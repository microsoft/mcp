// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Compute.Options.Vm;

public class VmSizesListOptions : SubscriptionOptions
{
    public string? Location { get; set; }
}
