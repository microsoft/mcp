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
        command.Options.Add(CommunicationOptionDefinitions.To);
        command.Options.Add(CommunicationOptionDefinitions.Cc);
        command.Options.Add(CommunicationOptionDefinitions.Bcc);
        command.Options.Add(CommunicationOptionDefinitions.Subject);
        command.Options.Add(CommunicationOptionDefinitions.Message);
        command.Options.Add(CommunicationOptionDefinitions.IsHtml);
        command.Options.Add(CommunicationOptionDefinitions.ReplyTo);
    }

    protected override EmailSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Sender = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Sender.Name);
        options.SenderName = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.SenderName.Name);
        options.To = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.To.Name);
        options.Cc = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Cc.Name);
        options.Bcc = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.Bcc.Name);
        options.Subject = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Subject.Name);
        options.Message = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Message.Name);
        options.IsHtml = parseResult.GetValueOrDefault<bool>(CommunicationOptionDefinitions.IsHtml.Name);
        options.ReplyTo = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.ReplyTo.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);
        var communicationService = context.GetService<ICommunicationService>();

        try
        {
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