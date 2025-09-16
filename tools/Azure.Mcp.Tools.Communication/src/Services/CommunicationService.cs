// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Communication.Sms;
using AzureMcp.Communication.Models;
using AzureMcp.Core.Options;
using AzureMcp.Core.Services.Azure;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication.Services;

public class CommunicationService(ILogger<CommunicationService> logger) : ICommunicationService
{
    private readonly ILogger<CommunicationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<List<SmsResult>> SendSmsAsync(
        string connectionString,
        string from,
        string[] to,
        string message,
        bool enableDeliveryReport = false,
        string? tag = null,
        RetryPolicyOptions? retryPolicy = null)
    {
        ValidateRequiredParameters(connectionString, from, to, message);

        try
        {
            var smsClient = new SmsClient(connectionString);

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

    private static void ValidateRequiredParameters(string connectionString, string from, string[] to, string message)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));

        if (string.IsNullOrWhiteSpace(from))
            throw new ArgumentException("From phone number cannot be null or empty", nameof(from));

        if (to == null || to.Length == 0)
            throw new ArgumentException("To phone numbers cannot be null or empty", nameof(to));

        if (to.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("All To phone numbers must be valid", nameof(to));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be null or empty", nameof(message));
    }
}