// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Azure.Mcp.Tools.Communication.Options.Sms;

public class SmsSendOptions : BaseCommunicationOptions
{
    [JsonPropertyName(CommunicationOptionDefinitions.FromName)]
    public string? From { get; set; }

    [JsonPropertyName(CommunicationOptionDefinitions.ToName)]
    public string[]? To { get; set; }

    [JsonPropertyName(CommunicationOptionDefinitions.MessageName)]
    public string? Message { get; set; }

    [JsonPropertyName(CommunicationOptionDefinitions.EnableDeliveryReportName)]
    public bool EnableDeliveryReport { get; set; }

    [JsonPropertyName(CommunicationOptionDefinitions.TagName)]
    public string? Tag { get; set; }
}
