using Azure.Mcp.Tools.ConfidentialLedger.Commands.Entries;
using Azure.Mcp.Tools.ConfidentialLedger.Models;
using Azure.Mcp.Tools.ConfidentialLedger.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ConfidentialLedger.UnitTests;

public sealed class LedgerEntryGetCommandTests : CommandUnitTestsBase<LedgerEntryGetCommand, IConfidentialLedgerService>
{
    [Fact]
    public async Task Execute_WithTransactionId_Success_ReturnsResult()
    {
        Service.GetLedgerEntryAsync("ledger1", "2.199", null, Arg.Any<CancellationToken>())
            .Returns(new LedgerEntryGetResult
            {
                LedgerName = "ledger1",
                TransactionId = "2.199",
                Contents = "{\"hello\":\"world\"}"
            });

        var response = await ExecuteCommandAsync("--ledger", "ledger1", "--transaction-id", "2.199");

        var result = ValidateAndConvertResponse(response, ConfidentialLedgerJsonContext.Default.LedgerEntryGetResult);
        Assert.Equal("2.199", result!.TransactionId);

        await Service.Received(1).GetLedgerEntryAsync("ledger1", "2.199", null, Arg.Any<CancellationToken>());
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
        await Assert.ThrowsAsync<ArgumentException>(() =>
            Service.GetLedgerEntryAsync(ledgerName!, transactionId!, null, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("attacker.com#")]
    [InlineData("evil.com/path#")]
    [InlineData("bad@host")]
    [InlineData("has space")]
    [InlineData("has.dot")]
    [InlineData("name#fragment")]
    [InlineData("name?query")]
    [InlineData("host:8080")]
    [InlineData("1startswithnumber")]
    [InlineData("-startswithhyphen")]
    public async Task GetLedgerEntryAsync_RejectsInvalidLedgerNames_PreventingSsrf(string ledgerName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            Service.GetLedgerEntryAsync(ledgerName, "1.0", null, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData("attacker.com#")]
    [InlineData("evil.com/path#")]
    [InlineData("bad@host")]
    [InlineData("name#fragment")]
    public async Task AppendEntryAsync_RejectsInvalidLedgerNames_PreventingSsrf(string ledgerName)
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            Service.AppendEntryAsync(ledgerName, "data", null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Execute_WithTransactionId_WithCollectionId_Success_ReturnsResult()
    {
        Service.GetLedgerEntryAsync("ledger1", "2.199", "my-collection", Arg.Any<CancellationToken>())
            .Returns(new LedgerEntryGetResult
            {
                LedgerName = "ledger1",
                TransactionId = "2.199",
                Contents = "{\"hello\":\"world\"}"
            });

        var response = await ExecuteCommandAsync(
            "--ledger", "ledger1",
            "--transaction-id", "2.199",
            "--collection-id", "my-collection");

        var result = ValidateAndConvertResponse(response, ConfidentialLedgerJsonContext.Default.LedgerEntryGetResult);
        Assert.Equal("2.199", result!.TransactionId);

        await Service.Received(1).GetLedgerEntryAsync("ledger1", "2.199", "my-collection", Arg.Any<CancellationToken>());
    }
}
