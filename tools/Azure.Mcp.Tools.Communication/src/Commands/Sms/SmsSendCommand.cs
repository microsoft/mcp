// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using AzureMcp.Communication.Models;
using AzureMcp.Communication.Options;
using AzureMcp.Communication.Options.Sms;
using AzureMcp.Communication.Services;
using AzureMcp.Core.Commands;
using AzureMcp.Core.Models.Command;
using Microsoft.Extensions.Logging;

namespace  Azure.Mcp.Tools.Communication.Commands.Sms;

public sealed class SmsSendCommand(ILogger<SmsSendCommand> logger) : BaseCommunicationCommand<SmsSendOptions>
{
    private const string CommandTitle = "Send SMS Message";
    private readonly ILogger<SmsSendCommand> _logger = logger;

    // Define options from OptionDefinitions
    private readonly Option<string> _fromOption = CommunicationOptionDefinitions.From;
    private readonly Option<string[]> _toOption = CommunicationOptionDefinitions.To;
    private readonly Option<string> _messageOption = CommunicationOptionDefinitions.Message;
    private readonly Option<bool> _enableDeliveryReportOption = CommunicationOptionDefinitions.EnableDeliveryReport;
    private readonly Option<string> _tagOption = CommunicationOptionDefinitions.Tag;

    public override string Name => "send";

    public override string Description =>
        """
        Send an SMS message using Azure Communication Services.
        
        Sends SMS messages to one or more recipients using your Communication Services resource.
        Supports delivery reporting and custom tags for message tracking.
        
        Required options:
        - --connection-string: Azure Communication Services connection string
        - --from: SMS-enabled phone number (E.164 format, e.g., +14255550123)
        - --to: Recipient phone number(s) (E.164 format)
        - --message: SMS message content
        
        Returns: Array of SMS send results with message IDs and delivery status.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_fromOption);
        command.AddOption(_toOption);
        command.AddOption(_messageOption);
        command.AddOption(_enableDeliveryReportOption);
        command.AddOption(_tagOption);
    }

    protected override SmsSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.From = parseResult.GetValueForOption(_fromOption);
        options.To = parseResult.GetValueForOption(_toOption);
        options.Message = parseResult.GetValueForOption(_messageOption);
        options.EnableDeliveryReport = parseResult.GetValueForOption(_enableDeliveryReportOption);
        options.Tag = parseResult.GetValueForOption(_tagOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            // Required validation step
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            // Get the Communication service from DI
            var communicationService = context.GetService<ICommunicationService>();

            // Call service operation with required parameters
            var results = await communicationService.SendSmsAsync(
                options.ConnectionString!,
                options.From!,
                options.To!,
                options.Message!,
                options.EnableDeliveryReport,
                options.Tag,
                options.RetryPolicy);

            // Set results
            context.Response.Results = results?.Count > 0 ?
                ResponseResult.Create(
                    new SmsSendCommandResult(results),
                    CommunicationJsonContext.Default.SmsSendCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error sending SMS. From: {From}, To: {To}, Message Length: {MessageLength}, Options: {@Options}",
                options.From, options.To != null ? string.Join(",", options.To) : "null",
                options.Message?.Length ?? 0, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Implementation-specific error handling
    protected override string GetErrorMessage(Exception ex) => ex switch
    {
        ArgumentException argEx => $"Invalid parameter: {argEx.Message}",
        Azure.RequestFailedException reqEx when reqEx.Status == 401 =>
            "Authentication failed. Please verify your connection string is correct and has not expired.",
        Azure.RequestFailedException reqEx when reqEx.Status == 403 =>
            "Authorization failed. Ensure your Communication Services resource has SMS permissions and the phone number is provisioned.",
        Azure.RequestFailedException reqEx when reqEx.Status == 400 =>
            $"Bad request: {reqEx.Message}. Please verify phone numbers are in E.164 format and message content is valid.",
        Azure.RequestFailedException reqEx => reqEx.Message,
        _ => base.GetErrorMessage(ex)
    };

    protected override int GetStatusCode(Exception ex) => ex switch
    {
        ArgumentException => 400,
        Azure.RequestFailedException reqEx => reqEx.Status,
        _ => base.GetStatusCode(ex)
    };

    // Strongly-typed result record
    internal record SmsSendCommandResult(List<SmsResult> Results);
}