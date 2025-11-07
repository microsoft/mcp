// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Deploy.Options.App;

public class LogsGetOptions : SubscriptionOptions
{
    [JsonPropertyName("workspaceFolder")]
    public string WorkspaceFolder { get; set; } = string.Empty;

    public string? ResourceGroupName => ResourceGroup;

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }
}
