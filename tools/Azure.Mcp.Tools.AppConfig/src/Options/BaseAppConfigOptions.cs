// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.AppConfig.Options;

public class BaseAppConfigOptions : SubscriptionOptions
{
    [JsonPropertyName(AppConfigOptionDefinitions.AccountName)]
    public string? Account { get; set; }
}
