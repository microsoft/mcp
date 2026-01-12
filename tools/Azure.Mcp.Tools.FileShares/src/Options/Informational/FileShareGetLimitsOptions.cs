// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands.Subscription;

namespace Azure.Mcp.Tools.FileShares.Options.Informational;

public class FileShareGetLimitsOptions : SubscriptionOptions
{
    public string? Location { get; set; }
}
