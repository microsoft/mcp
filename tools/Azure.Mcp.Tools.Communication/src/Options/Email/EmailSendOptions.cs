// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Options;

namespace Azure.Mcp.Tools.Communication.Options.Email;

/// <summary>
/// Options for the Email Send command.
/// </summary>
public sealed class EmailSendOptions : BaseCommunicationOptions
{
    /// <summary>
    /// The email address to send from (must be from a verified domain).
    /// </summary>
    [Option(Description = "The email address to send from (must be from a verified domain)")]
    public required string From { get; set; }

    /// <summary>
    /// The display name of the sender.
    /// </summary>
    [Option(Description = "The display name of the sender")]
    public string? SenderName { get; set; }

    /// <summary>
    /// The recipient email addresses.
    /// </summary>
    [Option(Description = "The recipient email address(es) to send the email to.")]
    public required string[] To { get; set; }

    /// <summary>
    /// Optional CC recipient email addresses.
    /// </summary>
    [Option(Description = "CC recipient email addresses")]
    public string[]? Cc { get; set; }

    /// <summary>
    /// Optional BCC recipient email addresses.
    /// </summary>
    [Option(Description = "BCC recipient email addresses")]
    public string[]? Bcc { get; set; }

    /// <summary>
    /// The email subject.
    /// </summary>
    [Option(Description = "The email subject")]
    public required string Subject { get; set; }

    /// <summary>
    /// The email body content.
    /// </summary>
    [Option(Description = "The email message content to send to the recipient(s).")]
    public required string Message { get; set; }

    /// <summary>
    /// Flag indicating whether the message content is HTML.
    /// </summary>
    [Option(Description = "Flag indicating whether the message content is HTML")]
    public bool IsHtml { get; set; }

    /// <summary>
    /// Optional reply-to addresses.
    /// </summary>
    [Option(Description = "Reply-to email addresses")]
    public string[]? ReplyTo { get; set; }
}
