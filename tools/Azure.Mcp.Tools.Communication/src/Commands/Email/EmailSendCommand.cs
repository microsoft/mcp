// Copyright (c) Microsoft Corporation
using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading.Tasks;
using Azure.Mcp.Commands;
using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication.Commands.Email;

/// <summary>
/// Send an email message using Azure Communication Services.
/// </summary>
public sealed class EmailSendCommand(ILogger<EmailSendCommand> logger) : BaseCommand<EmailSendOptions>(logger)
{

    private readonly ILogger<EmailSendCommand> _logger = logger;
    private const string CommandTitle = "Send Email";
    /// <inheritdoc/>
    public override string Name => "send";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        Name = "azmcp_communication_email_send",
        DisplayName = "Send Email",
        Description = Description,
        Secret = false,
        Destructive = false,
        ReadOnly = false,
        OpenWorld = false,
        Idempotent = false,
        LocalRequired = false
    };

    /// <inheritdoc/>
    public override string Description => "Send an email message using Azure Communication Services.\r\n\r\n" +
        "Sends emails to one or more recipients using your Communication Services resource.\r\n" +
        "Supports HTML content and CC/BCC recipients.\r\n\r\n" +
        "Required options:\r\n" +
        "- --endpoint: Azure Communication Services endpoint\r\n" +
        "- --sender: Email address to send from (must be from a verified domain)\r\n" +
        "- --to: Recipient email address(es)\r\n" +
        "- --subject: Email subject\r\n" +
        "- --message: Email body content\r\n\r\n" +
        "Returns: Email operation details including message ID and status.";

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CommunicationOptionDefinitions.Endpoint.AsRequired());
        command.Options.Add(CommunicationOptionDefinitions.Sender.AsRequired());
        command.Options.Add(CommunicationOptionDefinitions.SenderName.AsOptional());
        command.Options.Add(CommunicationOptionDefinitions.To.AsRequired());
        command.Options.Add(CommunicationOptionDefinitions.Cc.AsOptional());
        command.Options.Add(CommunicationOptionDefinitions.Bcc.AsOptional());
        command.Options.Add(CommunicationOptionDefinitions.Subject.AsRequired());
        command.Options.Add(CommunicationOptionDefinitions.Message.AsRequired());
        command.Options.Add(CommunicationOptionDefinitions.IsHtml.AsOptional());
        command.Options.Add(CommunicationOptionDefinitions.ReplyTo.AsOptional());
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsOptional());
        command.Options.Add(OptionDefinitions.Common.Subscription.AsOptional());
    }

    /// <inheritdoc/>
    protected override EmailSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Endpoint.Name);
        options.Sender = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Sender.Name);
        options.SenderName = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.SenderName.Name);
        options.To = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.To.Name);
        options.Cc = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Cc.Name);
        options.Bcc = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Bcc.Name);
        options.Subject = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Subject.Name);
        options.Message = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Message.Name);
        options.IsHtml = parseResult.GetValueOrDefault<bool>(CommunicationOptionDefinitions.IsHtml.Name);
        options.ReplyTo = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.ReplyTo.Name);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.Subscription = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.Subscription.Name);
        return options;
    }

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        try
        {
            var options = BindOptions(parseResult);
            var communicationService = context.GetService<ICommunicationService>();

            var result = await communicationService.SendEmailAsync(
                options.Endpoint!,
                options.Sender!,
                options.SenderName,
                options.To!,
                options.Subject!,
                options.Message!,
                options.IsHtml,
                options.Cc,
                options.Bcc,
                options.ReplyTo,
                options.Subscription,
                options.ResourceGroup,
                options.RetryPolicy);

            context.Response.Results = ResponseResult.Create(new EmailSendCommandResult(result), CommunicationJsonContext.Default.EmailSendCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, parseResult);
            HandleException(context, ex);
        }

        return context.Response;
    }

    /// <summary>
    /// Result returned by the Email Send Command.
    /// </summary>
    public record EmailSendCommandResult(EmailSendResult Result);
}