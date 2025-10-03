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

    /// <summary>
/// The endpoint URI for the Azure Communication Services resource.
/// </summary>
public static readonly OptionDefinition Endpoint = new(
    "--endpoint",
    "The endpoint URI for the Azure Communication Services resource (e.g., https://resourcename.communication.azure.com)");

/// <summary>
/// The email address to send from.
/// </summary>
public static readonly OptionDefinition Sender = new(
    "--sender",
    "The email address to send from (must be from a verified domain)");

/// <summary>
/// The display name of the sender.
/// </summary>
public static readonly OptionDefinition SenderName = new(
    "--sender-name",
    "The display name of the sender");

/// <summary>
/// The email subject.
/// </summary>
public static readonly OptionDefinition Subject = new(
    "--subject",
    "The email subject");

/// <summary>
/// Flag indicating whether the message content is HTML.
/// </summary>
public static readonly OptionDefinition IsHtml = new(
    "--is-html",
    "Flag indicating whether the message content is HTML");

/// <summary>
/// CC recipient email addresses.
/// </summary>
public static readonly OptionDefinition Cc = new(
    "--cc",
    "CC recipient email addresses");

/// <summary>
/// BCC recipient email addresses.
/// </summary>
public static readonly OptionDefinition Bcc = new(
    "--bcc",
    "BCC recipient email addresses");

/// <summary>
/// Reply-to email addresses.
/// </summary>
public static readonly OptionDefinition ReplyTo = new(
    "--reply-to",
    "Reply-to email addresses");
}
