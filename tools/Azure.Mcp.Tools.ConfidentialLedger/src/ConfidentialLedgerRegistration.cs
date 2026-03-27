// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.ConfidentialLedger;

public sealed class ConfidentialLedgerRegistration : IAreaRegistration
{
    public static string AreaName => "confidentialledger";

    public static string AreaTitle => "Azure Confidential Ledger";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure Confidential Ledger operations - Commands for appending and querying tamper-proof ledger entries backed by TEEs and blockchain-style integrity guarantees. Use these commands to write immutable audit records.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "entries",
                Description = "Ledger entries operations - Commands for appending and retrieving ledger entries.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "94fec47b-eb44-4d20-862f-24c284328956",
                        Name = "append",
                        Description = "Appends a tamper-proof entry to a Confidential Ledger instance and returns the transaction identifier.",
                        Title = "Append",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = false,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "ledger",
                                Description = "The name of the Confidential Ledger instance (e.g., 'myledger').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "content",
                                Description = "The JSON or text payload to append as a tamper-proof ledger entry.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "collection-id",
                                Description = "Optional ledger collection identifier. If omitted the default collection is used.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(LedgerEntryAppendCommand)
                    },
                    new CommandDescriptor
                    {
                        Id = "f1281e49-6392-455d-8caf-eb58428e8f5e",
                        Name = "get",
                        Description = "Retrieves the Confidential Ledger entry and its recorded contents for the specified transaction ID, optionally scoped to a collection.",
                        Title = "Get",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = false,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "ledger",
                                Description = "The name of the Confidential Ledger instance (e.g., 'myledger').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "transaction-id",
                                Description = "The Confidential Ledger transaction identifier (for example: '2.199').",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "collection-id",
                                Description = "Optional ledger collection identifier. If omitted the default collection is used.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Global,
                        HandlerType = nameof(LedgerEntryGetCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IConfidentialLedgerService, ConfidentialLedgerService>();
        services.AddSingleton<LedgerEntryAppendCommand>();
        services.AddSingleton<LedgerEntryGetCommand>();
    }

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(LedgerEntryAppendCommand) => serviceProvider.GetRequiredService<LedgerEntryAppendCommand>(),
            nameof(LedgerEntryGetCommand) => serviceProvider.GetRequiredService<LedgerEntryGetCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in confidentialledger area.")
        };
}
