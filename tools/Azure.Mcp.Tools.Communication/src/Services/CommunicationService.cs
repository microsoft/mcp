// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Communication.Sms;
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
}
