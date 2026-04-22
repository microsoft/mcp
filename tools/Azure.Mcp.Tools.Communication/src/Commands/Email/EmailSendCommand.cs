// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.Communication.Commands.Email;

/// <summary>
/// Send an email message using Azure Communication Services.
/// </summary>
public sealed class EmailSendCommand(ILogger<EmailSendCommand> logger, ICommunicationService communicationService) : BaseCommunicationCommand<EmailSendOptions>
{
    private const string CommandTitle = "Send Email";
    private readonly ILogger<EmailSendCommand> _logger = logger;
    private readonly ICommunicationService _communicationService = communicationService;

    public override string Name => "send";
    public override string Id => "60f79b69-9e90-4f07-9bf4-bd4452f1143d";

    public override string Title => CommandTitle;

    public override string Description =>
        """
        Send emails to one or multiple recipients to the given email-address. The emails can be plain text or HTML formatted. You can include a subject, custom sender name, CC and BCC recipients, and reply-to addresses.
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true,
        OpenWorld = true,
        Idempotent = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CommunicationOptionDefinitions.Sender);
        command.Options.Add(CommunicationOptionDefinitions.SenderName);
        command.Options.Add(CommunicationOptionDefinitions.ToEmail);
        command.Options.Add(CommunicationOptionDefinitions.Cc);
        command.Options.Add(CommunicationOptionDefinitions.Bcc);
        command.Options.Add(CommunicationOptionDefinitions.Subject);
        command.Options.Add(CommunicationOptionDefinitions.EmailMessage);
        command.Options.Add(CommunicationOptionDefinitions.IsHtml);
        command.Options.Add(CommunicationOptionDefinitions.ReplyTo);
        command.Validators.Add(commandResult =>
        {
            var to = commandResult.GetValueOrDefault(CommunicationOptionDefinitions.ToEmail);
            if (to == null || to.Length == 0)
                commandResult.AddError("At least one 'to' email address must be provided.");
            else if (to.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("to email addresses cannot be empty.");

            var cc = commandResult.GetValueOrDefault(CommunicationOptionDefinitions.Cc);
            if (cc != null && cc.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("CC email addresses should not be empty if provided by user.");

            var bcc = commandResult.GetValueOrDefault(CommunicationOptionDefinitions.Bcc);
            if (bcc != null && bcc.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("BCC email addresses should not be empty if provided by user.");

            var replyTo = commandResult.GetValueOrDefault(CommunicationOptionDefinitions.ReplyTo);
            if (replyTo != null && replyTo.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("Reply-To email addresses should not be empty if provided by user.");
        });
    }

    protected override EmailSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.From = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.Sender);
        options.SenderName = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.SenderName);
        options.To = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.ToEmail);
        options.Cc = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.Cc);
        options.Bcc = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.Bcc);
        options.Subject = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.Subject);
        options.Message = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.EmailMessage);
        options.IsHtml = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.IsHtml);
        options.ReplyTo = parseResult.GetValueOrDefault(CommunicationOptionDefinitions.ReplyTo);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }
        var options = BindOptions(parseResult);

        try
        {
            var result = await _communicationService.SendEmailAsync(
                options.Endpoint!,
                options.From!,
                options.SenderName,
                options.To!,
                options.Subject!,
                options.Message!,
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
    public record EmailSendCommandResult(EmailSendResult Result);
}
