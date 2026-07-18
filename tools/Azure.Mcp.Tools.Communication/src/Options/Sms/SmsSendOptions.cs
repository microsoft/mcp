// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Communication.Options.Sms;

public sealed class SmsSendOptions : BaseCommunicationOptions
{
    [Option(Description = "The SMS-enabled phone number associated with your Communication Services resource (in E.164 format, e.g., +14255550123). Can also be a short code or alphanumeric sender ID.")]
    public required string From { get; set; }

    [Option(Description = "The recipient phone number(s) in E.164 international standard format (e.g., +14255550123). Multiple numbers can be provided.")]
    public required string[] To { get; set; }

    [Option(Description = "The SMS message content to send to the recipient(s).")]
    public required string Message { get; set; }

    [Option(Description = "Whether to enable delivery reporting for the SMS message. When enabled, events are emitted when delivery is successful.")]
    public bool EnableDeliveryReport { get; set; }

    [Option(Description = "Optional custom tag to apply to the SMS message for tracking purposes.")]
    public string? Tag { get; set; }
}
