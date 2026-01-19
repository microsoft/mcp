// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;

namespace Azure.Mcp.Tools.FileShares.Options.Informational;

public class FileShareGetUsageDataOptions : SubscriptionOptions
{
    public string? Location { get; set; }
}
