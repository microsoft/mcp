// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options.Email;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Communication.Commands.Email;

/// <summary>
/// Send an email message using Azure Communication Services.
/// </summary>
[CommandMetadata(
    Id = "60f79b69-9e90-4f07-9bf4-bd4452f1143d",
    Name = "send",
    Title = "Send Email",
    Description = "Send emails to one or multiple recipients to the given email-address. The emails can be plain text or HTML formatted. You can include a subject, custom sender name, CC and BCC recipients, and reply-to addresses.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = true,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class EmailSendCommand(ILogger<EmailSendCommand> logger, ICommunicationService communicationService)
    : AuthenticatedCommand<EmailSendOptions, EmailSendCommand.EmailSendCommandResult>
{
    private readonly ILogger<EmailSendCommand> _logger = logger;
    private readonly ICommunicationService _communicationService = communicationService;

    public override void ValidateOptions(EmailSendOptions options, ValidationResult validationResult)
    {
        base.ValidateOptions(options, validationResult);

        if (options.To == null || options.To.Length == 0)
        {
            validationResult.Errors.Add("At least one 'to' email address must be provided.");
        }
        else if (options.To.Any(string.IsNullOrWhiteSpace))
        {
            validationResult.Errors.Add("to email addresses cannot be empty.");
        }

        if (options.Cc != null && options.Cc.Any(string.IsNullOrWhiteSpace))
        {
            validationResult.Errors.Add("CC email addresses should not be empty if provided by user.");
        }

        if (options.Bcc != null && options.Bcc.Any(string.IsNullOrWhiteSpace))
        {
            validationResult.Errors.Add("BCC email addresses should not be empty if provided by user.");
        }

        if (options.ReplyTo != null && options.ReplyTo.Any(string.IsNullOrWhiteSpace))
        {
            validationResult.Errors.Add("Reply-To email addresses should not be empty if provided by user.");
        }
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, EmailSendOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _communicationService.SendEmailAsync(
                options.Endpoint,
                options.From,
                options.SenderName,
                options.To,
                options.Subject,
                options.Message,
                options.IsHtml,
                options.Cc,
                options.Bcc,
                options.ReplyTo,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(result), CommunicationJsonContext.Default.EmailSendCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Endpoint: {Endpoint}.", Name, options.Endpoint);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Result returned by the Email Send Command.
    /// </summary>
    public sealed record EmailSendCommandResult(EmailSendResult Result);
}
