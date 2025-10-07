// Copyright (c) Microsoft Corporation
using Azure.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Communication.Options;

/// <summary>
/// Options for the Email Send command.
/// </summary>
public class EmailSendOptions : BaseCommunicationOptions
{
    /// <summary>
    /// The email address to send from (must be from a verified domain).
    /// </summary>
    public string? Sender { get; set; }

    /// <summary>
    /// The display name of the sender.
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// The recipient email addresses.
    /// </summary>
    public string[]? To { get; set; }

    /// <summary>
    /// Optional CC recipient email addresses.
    /// </summary>
    public string[]? Cc { get; set; }

    /// <summary>
    /// Optional BCC recipient email addresses.
    /// </summary>
    public string[]? Bcc { get; set; }

    /// <summary>
    /// The email subject.
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// The email body content.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Flag indicating whether the message content is HTML.
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Optional reply-to addresses.
    /// </summary>
    public string[]? ReplyTo { get; set; }
}
