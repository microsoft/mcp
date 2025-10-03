// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;

namespace Azure.Mcp.Tools.Communication.Options;

public static class CommunicationOptionDefinitions
{
    public const string EndpointName = "endpoint";
    public const string FromName = "from";
    public const string ToName = "to";
    public const string MessageName = "message";
    public const string EnableDeliveryReportName = "enable-delivery-report";
    public const string TagName = "tag";

    public static readonly Option<string> Endpoint = new(
        $"--{EndpointName}"
    )
    {
        Description = "The Communication Services URI endpoint (e.g., https://myservice.communication.azure.com). Required for credential authentication.",
        Required = true
    };

    public static readonly Option<string> From = new(
        $"--{FromName}"
    )
    {
        Description = "The SMS-enabled phone number associated with your Communication Services resource (in E.164 format, e.g., +14255550123). Can also be a short code or alphanumeric sender ID.",
        Required = true
    };

    public static readonly Option<string[]> To = new(
        $"--{ToName}"
    )
    {
        Description = "The recipient phone number(s) in E.164 international standard format (e.g., +14255550123). Multiple numbers can be provided.",
        Required = true
    };

    public static readonly Option<string> Message = new(
        $"--{MessageName}"
    )
    {
        Description = "The SMS message content to send to the recipient(s).",
        Required = true
    };

    public static readonly Option<bool> EnableDeliveryReport = new(
        $"--{EnableDeliveryReportName}"
    )
    {
        Description = "Whether to enable delivery reporting for the SMS message. When enabled, events are emitted when delivery is successful.",
        Required = false
    };

    public static readonly Option<string> Tag = new(
        $"--{TagName}"
    )
    {
        Description = "Optional custom tag to apply to the SMS message for tracking purposes.",
        Required = false
    };
}
