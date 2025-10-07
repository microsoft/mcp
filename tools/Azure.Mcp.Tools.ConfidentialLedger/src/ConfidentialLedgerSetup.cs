// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.ConfidentialLedger;

public class ConfidentialLedgerSetup : IAreaSetup
{
    public string Name => "confidentialledger";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IConfidentialLedgerService, ConfidentialLedgerService>();
        services.AddSingleton<LedgerEntryAppendCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var root = new CommandGroup(Name,
            "Azure Confidential Ledger operations - Commands for appending and querying tamper-proof ledger entries backed by TEEs and blockchain-style integrity guarantees. Use these commands to write immutable audit records.");

        var entries = new CommandGroup("entries", "Ledger entries operations - Commands for appending and retrieving ledger entries.");
        root.AddSubGroup(entries);

        var append = serviceProvider.GetRequiredService<LedgerEntryAppendCommand>();
        entries.AddCommand(append.Name, append);

        return root;
    }
}
