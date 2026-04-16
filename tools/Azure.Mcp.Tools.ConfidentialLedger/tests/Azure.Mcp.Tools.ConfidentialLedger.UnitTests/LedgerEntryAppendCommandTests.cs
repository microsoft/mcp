// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.UnitTests;

public class LedgerEntryAppendCommandTests : CommandUnitTestsBase<LedgerEntryAppendCommand, IConfidentialLedgerService>
{
    [Fact]
    public async Task Execute_Success_ReturnsResult()
    {
        Service.AppendEntryAsync("ledger1", "data", cancellationToken: Arg.Any<CancellationToken>())
            .Returns(new AppendEntryResult { TransactionId = "tx1", State = "Committed" });

        var response = await ExecuteCommandAsync("--ledger", "ledger1", "--content", "data");

        var result = ValidateAndConvertResponse(response, ConfidentialLedgerJsonContext.Default.AppendEntryResult);
        Assert.Equal("tx1", result.TransactionId);
    }
}
