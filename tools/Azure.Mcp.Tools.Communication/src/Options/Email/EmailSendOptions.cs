// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Core.Serialization;
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
    [JsonPropertyName("from")]
    public string? From { get; set; }

    /// <summary>
    /// The display name of the sender.
    /// </summary>
    [JsonPropertyName("sender-name")]
    public string? SenderName { get; set; }

    /// <summary>
    /// The recipient email addresses.
    /// </summary>
    [JsonPropertyName(CommunicationOptionDefinitions.ToEmailName)]
    public string[]? To { get; set; }

    /// <summary>
    /// Optional CC recipient email addresses.
    /// </summary>
    [JsonPropertyName("cc")]
    public string[]? Cc { get; set; }

    /// <summary>
    /// Optional BCC recipient email addresses.
    /// </summary>
    [JsonPropertyName("bcc")]
    public string[]? Bcc { get; set; }

    /// <summary>
    /// The email subject.
    /// </summary>
    [JsonPropertyName("subject")]
    public string? Subject { get; set; }

    /// <summary>
    /// The email body content.
    /// </summary>
    [JsonPropertyName(CommunicationOptionDefinitions.EmailMessageName)]
    public string? Message { get; set; }

    /// <summary>
    /// Flag indicating whether the message content is HTML.
    /// </summary>
    [JsonPropertyName("is-html")]
    public bool IsHtml { get; set; }

    /// <summary>
    /// Optional reply-to addresses.
    /// </summary>
    [JsonPropertyName("reply-to")]
    public string[]? ReplyTo { get; set; }
}
