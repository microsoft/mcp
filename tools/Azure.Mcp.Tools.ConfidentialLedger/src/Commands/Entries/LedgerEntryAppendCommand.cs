// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Tools.ConfidentialLedger.Options;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;

[CommandMetadata(
    Id = "94fec47b-eb44-4d20-862f-24c284328956",
    Name = "append",
    Title = "Append Confidential Ledger Entry",
    Description = "Appends a tamper-proof entry to a Confidential Ledger instance and returns the transaction identifier.",
    Destructive = false,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class LedgerEntryAppendCommand(IConfidentialLedgerService service, ILogger<LedgerEntryAppendCommand> logger)
    : BaseConfidentialLedgerCommand<AppendEntryOptions>
{
    private readonly IConfidentialLedgerService _service = service;
    private readonly ILogger<LedgerEntryAppendCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ConfidentialLedgerOptionDefinitions.Content.AsRequired());
        command.Options.Add(ConfidentialLedgerOptionDefinitions.CollectionId);
    }

    protected override AppendEntryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Content = parseResult.GetValueOrDefault<string>(ConfidentialLedgerOptionDefinitions.Content.Name);
        options.CollectionId = parseResult.GetValueOrDefault<string?>(ConfidentialLedgerOptionDefinitions.CollectionId.Name);
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
            var result = await _service.AppendEntryAsync(options.LedgerName!, options.Content!, options.CollectionId, cancellationToken);
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
