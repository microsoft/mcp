// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Communication.Commands.Sms;

[CommandMetadata(
    Id = "a0dc94f3-25ac-4971-a552-0d90fd57e902",
    Name = "send",
    Title = "Send SMS Message",
    Description = """
        Sends SMS messages to one or more recipients to the given phone-number. You can enable delivery reports and receipt tracking, broadcast SMS, and tag messages for easier tracking.
        Returns message IDs and delivery status for each sent message.
        """,
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class SmsSendCommand(ILogger<SmsSendCommand> logger, ICommunicationService communicationService)
    : AuthenticatedCommand<SmsSendOptions, SmsSendCommandResult>
{
    private readonly ILogger<SmsSendCommand> _logger = logger;
    private readonly ICommunicationService _communicationService = communicationService;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, SmsSendOptions options, CancellationToken cancellationToken)
    {
        try
        {
            // Call service operation with required parameters
            var results = await _communicationService.SendSmsAsync(
                options.Endpoint,
                options.From,
                options.To,
                options.Message,
                options.EnableDeliveryReport,
                options.Tag,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            // Set results
            context.Response.Results = results?.Count > 0 ?
                ResponseResult.Create(new(results), CommunicationJsonContext.Default.SmsSendCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error sending SMS. From: {From}, To: {To}, Message Length: {MessageLength}.",
                options.From, options.To != null ? string.Join(",", options.To) : "null",
                options.Message?.Length ?? 0);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
