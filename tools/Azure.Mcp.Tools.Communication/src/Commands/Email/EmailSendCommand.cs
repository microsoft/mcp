// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Communication.Commands;
using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication.Commands.Email;

/// <summary>
/// Send an email message using Azure Communication Services.
/// </summary>
public sealed class EmailSendCommand(ILogger<EmailSendCommand> logger) : BaseCommunicationCommand<EmailSendOptions>
{
    private const string CommandTitle = "Send Email";
    private readonly ILogger<EmailSendCommand> _logger = logger;

    public override string Name => "send";

    public override string Title => CommandTitle;

    public override string Description =>
        """
        Send an email message using Azure Communication Services.
        
        Sends emails to one or more recipients using your Communication Services resource.
        Supports HTML content and CC/BCC recipients.
        """;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false,
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
            var to = commandResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.ToEmail.Name);
            if (to == null || to.Length == 0)
                commandResult.AddError("At least one 'to' email address must be provided.");
            else if (to.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("to email addresses cannot be empty.");

            var cc = commandResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Cc.Name);
            if (cc != null && cc.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("CC email addresses should not be empty if provided by user.");

            var bcc = commandResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Bcc.Name);
            if (bcc != null && bcc.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("BCC email addresses should not be empty if provided by user.");

            var replyTo = commandResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.ReplyTo.Name);
            if (replyTo != null && replyTo.Any(string.IsNullOrWhiteSpace))
                commandResult.AddError("Reply-To email addresses should not be empty if provided by user.");
        });
    }

    protected override EmailSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.From = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Sender.Name);
        options.SenderName = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.SenderName.Name);
        options.To = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.ToEmail.Name);
        options.Cc = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Cc.Name);
        options.Bcc = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Bcc.Name);
        options.Subject = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Subject.Name);
        options.Message = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.EmailMessage.Name);
        options.IsHtml = parseResult.GetValueOrDefault<bool>(CommunicationOptionDefinitions.IsHtml.Name);
        options.ReplyTo = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.ReplyTo.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }
        var options = BindOptions(parseResult);
        var communicationService = context.GetService<ICommunicationService>();

        try
        {
            var result = await communicationService.SendEmailAsync(
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
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new(result), CommunicationJsonContext.Default.EmailSendCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Result returned by the Email Send Command.
    /// </summary>
    public record EmailSendCommandResult(EmailSendResult Result);
}
