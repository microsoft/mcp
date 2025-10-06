// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.UnitTests;

public class LedgerEntryAppendCommandTests
{
    [Fact]
    public async Task Execute_Success_ReturnsResult()
    {
        var service = Substitute.For<IConfidentialLedgerService>();
        var logger = Substitute.For<ILogger<LedgerEntryAppendCommand>>();
        service.AppendEntryAsync("ledger1", "data")
            .Returns(new AppendEntryResult { TransactionId = "tx1", State = "Committed" });

        var provider = new ServiceCollection()
            .AddSingleton(service)
            .BuildServiceProvider();

        var command = new LedgerEntryAppendCommand(service, logger);
        var context = new CommandContext(provider);
        var parse = command.GetCommand().Parse(["--ledger", "ledger1", "--content", "data"]);
        var response = await command.ExecuteAsync(context, parse);

        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ConfidentialLedgerJsonContext.Default.AppendEntryResult);
        Assert.NotNull(result);
        Assert.Equal("tx1", result.TransactionId);
    }
}
