// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Core.Options;
using System.Text.Json.Serialization;

namespace Azure.Mcp.Core.Areas.Subscription.Options;

public class SubscriptionListOptions : GlobalOptions
{
    /// <summary>
    /// The maximum number of characters to return in the response. Defaults to 10000.
    /// </summary>
    [JsonPropertyName(OptionDefinitions.Common.CharacterLimitName)]
    public int CharacterLimit { get; set; } = 10000;
}
