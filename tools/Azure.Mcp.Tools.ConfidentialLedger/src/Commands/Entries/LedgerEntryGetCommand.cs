using System.CommandLine;
using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ConfidentialLedger.Options;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;

public sealed class LedgerEntryGetCommand(IConfidentialLedgerService service, ILogger<LedgerEntryGetCommand> logger)
    : BaseConfidentialLedgerCommand<GetEntryOptions>
{
    private const string CommandTitle = "Retrieve Confidential Ledger Entry";
    private readonly IConfidentialLedgerService _service = service;
    private readonly ILogger<LedgerEntryGetCommand> _logger = logger;

    public override string Name => "get";

    public override string Description =>
        "Retrieves the Confidential Ledger entry and its recorded contents for the specified transaction ID, optionally scoped to a collection.";

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        OpenWorld = false,
        Destructive = false,
        Idempotent = true,
        ReadOnly = true,
        Secret = false,
        LocalRequired = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(ConfidentialLedgerOptionDefinitions.TransactionId.AsRequired());
        command.Options.Add(ConfidentialLedgerOptionDefinitions.CollectionId);
    }

    protected override GetEntryOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.TransactionId = parseResult.GetValueOrDefault<string>(ConfidentialLedgerOptionDefinitions.TransactionId.Name);
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

        try
        {
            var result = await _service.GetLedgerEntryAsync(
                options.LedgerName!,
                options.TransactionId!,
                options.CollectionId).ConfigureAwait(false);

            context.Response.Results = ResponseResult.Create(
                result,
                ConfidentialLedgerJsonContext.Default.LedgerEntryGetResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ledger entry. Ledger: {Ledger} Transaction: {TransactionId}", options.LedgerName, options.TransactionId);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
