// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Text.Json.Serialization;
using AzureMcp.Core.Options;
namespace AzureMcp.AppService.Options;

public class BaseAppServiceOptions : SubscriptionOptions
{
    [JsonPropertyName(AppServiceOptionDefinitions.AppName)]
    public string? AppName { get; set; }
}
