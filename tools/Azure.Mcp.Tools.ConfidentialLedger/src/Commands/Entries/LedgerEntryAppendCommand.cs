// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.ConfidentialLedger.Options;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;

public sealed class LedgerEntryAppendCommand(IConfidentialLedgerService service, ILogger<LedgerEntryAppendCommand> logger)
    : BaseConfidentialLedgerCommand<AppendEntryOptions>
{
    private const string CommandTitle = "Append Confidential Ledger Entry";
    private readonly IConfidentialLedgerService _service = service;
    private readonly ILogger<LedgerEntryAppendCommand> _logger = logger;

    public override string Name => "append";

    public override string Description =>
        "Appends a tamper-proof entry to a Confidential Ledger instance and returns the transaction identifier.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        // Appending creates immutable data - not destructive but not idempotent.
        OpenWorld = false,
        Destructive = false,
        Idempotent = false,
        ReadOnly = false,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ConfidentialLedgerOptionDefinitions.Content);
        command.Options.Add(ConfidentialLedgerOptionDefinitions.CollectionId);
    }

    protected override AppendEntryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Content = parseResult.GetValueOrDefault<string>(ConfidentialLedgerOptionDefinitions.Content.Name);
        options.CollectionId = parseResult.GetValueOrDefault<string?>(ConfidentialLedgerOptionDefinitions.CollectionId.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        // Additional defensive validation (in case of external invocation bypassing parser requirements)
        if (string.IsNullOrWhiteSpace(options.LedgerName) || string.IsNullOrWhiteSpace(options.Content))
        {
            context.Response.Status = System.Net.HttpStatusCode.BadRequest;
            context.Response.Message = "Missing Required options: --ledger, --content";
            return context.Response;
        }

        try
        {
            var result = await _service.AppendEntryAsync(options.LedgerName!, options.Content!, options.CollectionId);
            context.Response.Results = ResponseResult.Create(result, ConfidentialLedgerJsonContext.Default.AppendEntryResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error appending ledger entry. Ledger: {Ledger}", options.LedgerName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
