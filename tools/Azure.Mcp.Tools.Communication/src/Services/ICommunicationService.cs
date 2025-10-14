// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Communication.Models;

namespace Azure.Mcp.Tools.Communication.Services;

public interface ICommunicationService
{
    Task<List<SmsResult>> SendSmsAsync(
        string endpoint,
        string from,
        string[] to,
        string message,
        bool enableDeliveryReport = false,
        string? tag = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);

    /// <summary>
    /// Sends an email using Azure Communication Services.
    /// </summary>
    /// <param name="endpoint">The Azure Communication Services endpoint.</param>
    /// <param name="from">The email address to send from (must be from a verified domain).</param>
    /// <param name="senderName">The display name of the sender.</param>
    /// <param name="to">The recipient email addresses.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="message">The email body content.</param>
    /// <param name="isHtml">Flag indicating whether the message content is HTML.</param>
    /// <param name="cc">Optional CC recipient email addresses.</param>
    /// <param name="bcc">Optional BCC recipient email addresses.</param>
    /// <param name="replyTo">Optional reply-to addresses.</param>
    /// <param name="retryPolicy">Optional retry policy options.</param>
    /// <returns>The result of the email send operation.</returns>
    Task<EmailSendResult> SendEmailAsync(
        string endpoint,
        string from,
        string? senderName,
        string[] to,
        string subject,
        string message,
        bool isHtml = false,
        string[]? cc = null,
        string[]? bcc = null,
        string[]? replyTo = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null);
}
