// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Communication.Sms;
using Azure.Communication.Email;
using Azure.Core;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Communication.Models;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication.Services;

public class CommunicationService(ISubscriptionService subscriptionService, ITenantService tenantService, ILogger<CommunicationService> logger)
    : BaseAzureService(tenantService), ICommunicationService
{
    private readonly ISubscriptionService _subscriptionService = subscriptionService ?? throw new ArgumentNullException(nameof(subscriptionService));
    private readonly ILogger<CommunicationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<SmsResult>> SendSmsAsync(
        string endpoint,
        string from,
        string[] to,
        string message,
        bool enableDeliveryReport = false,
        string? tag = null,
        string? tenantId = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        // Validate required parameters using base class method
        ValidateRequiredParameters(
            (nameof(endpoint), endpoint),
            (nameof(from), from),
            (nameof(message), message));

        // Validate to array separately since it has special requirements
        if (to == null || to.Length == 0)
            throw new ArgumentException("At least one 'to' phone number must be provided", nameof(to));

        if (to.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Recipient phone numbers cannot be empty.", nameof(to));

        try
        {
            // Create SMS client using Azure credential from base class and endpoint
            var credential = await GetCredential(tenantId);
            var smsClient = new SmsClient(new Uri(endpoint), credential);

            var sendOptions = new SmsSendOptions(enableDeliveryReport)
            {
                Tag = tag
            };

            _logger.LogInformation("Sending SMS from {From} to {ToCount} recipient(s)", from, to.Length);

            var response = await smsClient.SendAsync(
                from: from,
                to: to,
                message: message,
                options: sendOptions);

            var results = new List<SmsResult>();
            foreach (var result in response.Value)
            {
                results.Add(new SmsResult
                {
                    MessageId = result.MessageId,
                    To = result.To,
                    Successful = result.Successful,
                    HttpStatusCode = result.HttpStatusCode,
                    ErrorMessage = result.ErrorMessage
                });

                _logger.LogInformation("SMS to {To}: Success={Success}, MessageId={MessageId}, Status={Status}",
                    result.To, result.Successful, result.MessageId, result.HttpStatusCode);
            }

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS from {From} to {ToCount} recipient(s)", from, to.Length);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<EmailSendResult> SendEmailAsync(
        string endpoint,
        string sender,
        string? senderName,
        string[] to,
        string subject,
        string message,
        bool isHtml = false,
        string[]? cc = null,
        string[]? bcc = null,
        string[]? replyTo = null,
        string? subscription = null,
        string? resourceGroup = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        // Create email client with credential from base class
        var credential = await GetCredential(null);
        var emailClient = new EmailClient(new Uri(endpoint), credential);

        // Create the email content
        var emailContent = new EmailContent(subject);

        if (isHtml)
        {
            emailContent.Html = message;
        }
        else
        {
            emailContent.PlainText = message;
        }

        // Create the recipient list
        var recipientList = to.Select(address => new EmailAddress(address)).ToList();

        // Create the email message
        var emailMessage = new EmailMessage(
            senderAddress: new EmailAddress(sender, senderName),
            content: emailContent,
            recipients: new EmailRecipients(recipientList));

        // Add CC recipients if provided
        if (cc != null && cc.Length > 0)
        {
            emailMessage.Recipients.Cc = cc.Select(address => new EmailAddress(address)).ToList();
        }

        // Add BCC recipients if provided
        if (bcc != null && bcc.Length > 0)
        {
            emailMessage.Recipients.Bcc = bcc.Select(address => new EmailAddress(address)).ToList();
        }

        // Add reply-to addresses if provided
        if (replyTo != null && replyTo.Length > 0)
        {
            emailMessage.ReplyTo = replyTo.Select(address => new EmailAddress(address)).ToList();
        }

        // Send the email
        var response = await emailClient.SendAsync(
            WaitUntil.Started,
            emailMessage,
            CancellationToken.None);

        // Get the operation
        var operation = response.Value;

        // Check the status
        var operationResponse = await operation.UpdateStatusAsync();
        var operationResult = operationResponse.Value;

        return new EmailSendResult
        {
            MessageId = operationResult.MessageId,
            Status = operationResult.Status.ToString()
        };
    }
}
