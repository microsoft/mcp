// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Mcp.Tools.ConfidentialLedger.Options;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

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
    : AuthenticatedCommand<AppendEntryOptions, AppendEntryResult>
{
    private readonly IConfidentialLedgerService _service = service;
    private readonly ILogger<LedgerEntryAppendCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, AppendEntryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.AppendEntryAsync(options.Ledger, options.Content, options.CollectionId, cancellationToken);
            context.Response.Results = ResponseResult.Create(result, ConfidentialLedgerJsonContext.Default.AppendEntryResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error appending ledger entry. Ledger: {Ledger}", options.Ledger);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
