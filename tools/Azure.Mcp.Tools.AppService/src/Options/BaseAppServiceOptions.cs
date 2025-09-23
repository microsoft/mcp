// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Options;
namespace Azure.Mcp.Tools.AppService.Options;

public class BaseAppServiceOptions : SubscriptionOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.AppName)]
    public string? AppName { get; set; }
}
