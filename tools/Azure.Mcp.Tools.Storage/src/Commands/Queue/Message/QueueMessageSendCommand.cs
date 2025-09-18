// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Tools.Storage.Models;
using Azure.Mcp.Tools.Storage.Options;
using Azure.Mcp.Tools.Storage.Options.Queue.Message;
using Azure.Mcp.Tools.Storage.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.Storage.Commands.Queue.Message;

public sealed class QueueMessageSendCommand(ILogger<QueueMessageSendCommand> logger)
    : BaseQueueCommand<QueueMessageSendOptions>
{
    private const string CommandTitle = "Send message to Azure Storage queue";
    private readonly ILogger<QueueMessageSendCommand> _logger = logger;

    public override string Name => "send";

    public override string Description =>
        """
        Send messages to an Azure Storage queue for asynchronous processing. This tool sends a message to a specified queue
        with optional time-to-live and visibility delay settings. Messages are returned with receipt handles for tracking.
        Returns a QueueMessageSendResult object containing message ID, insertion time, expiration time, pop receipt,
        next visible time, and message content.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = false,
        OpenWorld = true,
        ReadOnly = false,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(StorageOptionDefinitions.Message);
        command.Options.Add(StorageOptionDefinitions.TimeToLiveInSeconds);
        command.Options.Add(StorageOptionDefinitions.VisibilityTimeoutInSeconds);
    }

    protected override QueueMessageSendOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Message = parseResult.GetValueOrDefault<string>(StorageOptionDefinitions.Message.Name);
        options.TimeToLiveInSeconds = parseResult.GetValueOrDefault<int?>(StorageOptionDefinitions.TimeToLiveInSeconds.Name);
        options.VisibilityTimeoutInSeconds = parseResult.GetValueOrDefault<int?>(StorageOptionDefinitions.VisibilityTimeoutInSeconds.Name);
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
            // Get the storage service from DI
            var service = context.GetService<IStorageService>();

            // Call service operation with required parameters
            var result = await service.SendQueueMessage(
                options.Account!,
                options.Queue!,
                options.Message!,
                options.TimeToLiveInSeconds,
                options.VisibilityTimeoutInSeconds,
                options.Subscription!,
                options.Tenant,
                options.RetryPolicy);

            // Set results
            context.Response.Results = result != null ?
                ResponseResult.Create(new(result), StorageJsonContext.Default.QueueMessageSendCommandResult) :
                null;
        }
        catch (Exception ex)
        {
            // Log error with all relevant context
            _logger.LogError(ex,
                "Error in {Operation}. Account: {Account}, Queue: {Queue}, Options: {@Options}",
                Name, options.Account, options.Queue, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    // Strongly-typed result record
    internal record QueueMessageSendCommandResult([property: JsonPropertyName("message")] QueueMessageSendResult Message);
}
