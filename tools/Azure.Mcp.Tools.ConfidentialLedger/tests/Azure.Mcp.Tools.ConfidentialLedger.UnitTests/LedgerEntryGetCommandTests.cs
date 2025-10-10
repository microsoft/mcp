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

public sealed class LedgerEntryGetCommandTests
{
    [Fact]
    public async Task Execute_WithTransactionId_Success_ReturnsResult()
    {
        var service = Substitute.For<IConfidentialLedgerService>();
        var logger = Substitute.For<ILogger<LedgerEntryGetCommand>>();

        service.GetLedgerEntryAsync("ledger1", "2.199", null)
            .Returns(new LedgerEntryGetResult
            {
                LedgerName = "ledger1",
                TransactionId = "2.199",
                Contents = "{\"hello\":\"world\"}"
            });

        var provider = new ServiceCollection()
            .AddSingleton(service)
            .BuildServiceProvider();

        var command = new LedgerEntryGetCommand(service, logger);
        var context = new CommandContext(provider);
        var parse = command.GetCommand().Parse(["--ledger", "ledger1", "--transaction-id", "2.199"]);

        var response = await command.ExecuteAsync(context, parse);

        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ConfidentialLedgerJsonContext.Default.LedgerEntryGetResult);
        Assert.NotNull(result);
        Assert.Equal("2.199", result!.TransactionId);

        await service.Received(1).GetLedgerEntryAsync("ledger1", "2.199", null);
    }

    [Theory]
    [InlineData(null, "transactionId")]
    [InlineData("", "transactionId")]
    [InlineData(" ", "transactionId")]
    [InlineData("ledgerName", null)]
    [InlineData("ledgerName", "")]
    [InlineData("ledgerName", " ")]
    public async Task GetLedgerEntryAsync_ThrowsArgumentNullException_WhenParametersInvalid(string? ledgerName, string? transactionId)
    {
        var service = new ConfidentialLedgerService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.GetLedgerEntryAsync(ledgerName!, transactionId!, null));
    }

    [Fact]
    public async Task Execute_WithTransactionId_WithCollectionId_Success_ReturnsResult()
    {
        var service = Substitute.For<IConfidentialLedgerService>();
        var logger = Substitute.For<ILogger<LedgerEntryGetCommand>>();

        service.GetLedgerEntryAsync("ledger1", "2.199", "my-collection")
            .Returns(new LedgerEntryGetResult
            {
                LedgerName = "ledger1",
                TransactionId = "2.199",
                Contents = "{\"hello\":\"world\"}"
            });

        var provider = new ServiceCollection()
            .AddSingleton(service)
            .BuildServiceProvider();

        var command = new LedgerEntryGetCommand(service, logger);
        var context = new CommandContext(provider);
        var parse = command.GetCommand().Parse(["--ledger", "ledger1", "--transaction-id", "2.199", "--collection-id", "my-collection"]);

        var response = await command.ExecuteAsync(context, parse);

        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ConfidentialLedgerJsonContext.Default.LedgerEntryGetResult);
        Assert.NotNull(result);
        Assert.Equal("2.199", result!.TransactionId);

        await service.Received(1).GetLedgerEntryAsync("ledger1", "2.199", "my-collection");
    }
}
