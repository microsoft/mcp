// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.Communication.Models;
using Azure.Mcp.Tools.Communication.Options;
using Azure.Mcp.Tools.Communication.Options.Sms;
using Azure.Mcp.Tools.Communication.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Communication.Commands.Sms;

public sealed class SmsSendCommand(ILogger<SmsSendCommand> logger) : BaseCommunicationCommand<SmsSendOptions>
{
    private const string CommandTitle = "Send SMS Message";
    private readonly ILogger<SmsSendCommand> _logger = logger;

    public override string Name => "send";

    public override string Description =>
        """
        Sends SMS messages to one or more recipients using Azure Communication Services.
        Returns message IDs and delivery status for each sent message.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        ReadOnly = true,
        OpenWorld = true,
        Idempotent = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(CommunicationOptionDefinitions.From);
        command.Options.Add(CommunicationOptionDefinitions.To);
        command.Options.Add(CommunicationOptionDefinitions.Message);
        command.Options.Add(CommunicationOptionDefinitions.EnableDeliveryReport);
        command.Options.Add(CommunicationOptionDefinitions.Tag);
    }

    protected override SmsSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.From = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.From.Name);
        options.To = parseResult.GetValueOrDefault<string[]>(CommunicationOptionDefinitions.To.Name);
        options.Message = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Message.Name);
        options.EnableDeliveryReport = parseResult.GetValueOrDefault<bool>(CommunicationOptionDefinitions.EnableDeliveryReport.Name);
        options.Tag = parseResult.GetValueOrDefault<string>(CommunicationOptionDefinitions.Tag.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            // Get the Communication service from DI
            var communicationService = context.GetService<ICommunicationService>();

            // Call service operation with required parameters
            var results = await communicationService.SendSmsAsync(
                options.Endpoint!,
                options.From!,
                options.To!,
                options.Message!,
                options.EnableDeliveryReport,
                options.Tag,
                options.Tenant,
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

    // Result type moved to Models/SmsSendCommandResult.cs
}
