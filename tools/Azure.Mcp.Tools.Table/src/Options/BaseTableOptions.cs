// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Table.Options;

public class BaseTableOptions : SubscriptionOptions
{
    public string? Account { get; set; }
}
