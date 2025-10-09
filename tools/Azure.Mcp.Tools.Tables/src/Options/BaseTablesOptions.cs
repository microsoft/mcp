// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Tables.Options;

public class BaseTablesOptions : SubscriptionOptions
{
    public string? Account { get; set; }
}
