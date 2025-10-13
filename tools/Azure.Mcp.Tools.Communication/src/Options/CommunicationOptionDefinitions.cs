// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace Azure.Mcp.Tools.Communication.Options;

public static class CommunicationOptionDefinitions
{
    public const string EndpointName = "endpoint";
    public const string FromSmsName = "from";
    public const string FromEmailName = "from";
    public const string ToName = "to";
    public const string ToEmailName = "to";
    public const string EmailMessageName = "message";
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
        $"--{FromSmsName}"
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
        Required = true,
        Arity = ArgumentArity.OneOrMore
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

    public static readonly Option<string[]> ToEmail = new(
        $"--{ToEmailName}"
    )
    {
        Description = "The recipient email address(es) to send the email to.",
        Required = true,
        Arity = ArgumentArity.OneOrMore
    };

    public static readonly Option<string> EmailMessage = new(
        $"--{EmailMessageName}"
    )
    {
        Description = "The email message content to send to the recipient(s).",
        Required = true,
    };
    /// <summary>
    /// The email address to send from.
    /// </summary>
    public static readonly Option<string> Sender = new(
        $"--{FromEmailName}"
    )
    {
        Description = "The email address to send from (must be from a verified domain)",
        Required = true
    };

    /// <summary>
    /// The display name of the sender.
    /// </summary>
    public static readonly Option<string> SenderName = new(
        "--sender-name"
    )
    {
        Description = "The display name of the sender",
        Required = false
    };

    /// <summary>
    /// The email subject.
    /// </summary>
    public static readonly Option<string> Subject = new(
        "--subject"
    )
    {
        Description = "The email subject",
        Required = true
    };

    /// <summary>
    /// Flag indicating whether the message content is HTML.
    /// </summary>
    public static readonly Option<bool> IsHtml = new(
        "--is-html"
    )
    {
        Description = "Flag indicating whether the message content is HTML",
        Required = false
    };

    /// <summary>
    /// CC recipient email addresses.
    /// </summary>
    public static readonly Option<string[]> Cc = new(
        "--cc"
    )
    {
        Description = "CC recipient email addresses",
        Required = false,
        Arity = ArgumentArity.ZeroOrMore
    };

    /// <summary>
    /// BCC recipient email addresses.
    /// </summary>
    public static readonly Option<string[]> Bcc = new(
        "--bcc"
    )
    {
        Description = "BCC recipient email addresses",
        Required = false,
        Arity = ArgumentArity.ZeroOrMore
    };

    /// <summary>
    /// Reply-to email addresses.
    /// </summary>
    public static readonly Option<string[]> ReplyTo = new(
        "--reply-to"
    )
    {
        Description = "Reply-to email addresses",
        Required = false,
        Arity = ArgumentArity.ZeroOrMore
    };
}
