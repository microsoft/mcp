// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Mcp.Tools.ConfidentialLedger.Options;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;

[CommandMetadata(
    Id = "f1281e49-6392-455d-8caf-eb58428e8f5e",
    Name = "get",
    Title = "Retrieve Confidential Ledger Entry",
    Description = "Retrieves the Confidential Ledger entry and its recorded contents for the specified transaction ID, optionally scoped to a collection.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    Secret = false,
    LocalRequired = false)]
public sealed class LedgerEntryGetCommand(IConfidentialLedgerService service, ILogger<LedgerEntryGetCommand> logger)
    : AuthenticatedCommand<GetEntryOptions, LedgerEntryGetResult>
{
    private readonly IConfidentialLedgerService _service = service;
    private readonly ILogger<LedgerEntryGetCommand> _logger = logger;

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, GetEntryOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetLedgerEntryAsync(
                options.Ledger,
                options.TransactionId,
                options.CollectionId,
                cancellationToken).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ConfidentialLedgerJsonContext.Default.LedgerEntryGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ledger entry. Ledger: {Ledger} Transaction: {TransactionId}", options.Ledger, options.TransactionId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
