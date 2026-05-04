// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.FileShares.Options.Informational;

public class FileShareGetUsageDataOptions : SubscriptionOptions
{
    public string? Location { get; set; }
}
