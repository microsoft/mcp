// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Monitor.Commands;
using Azure.Mcp.Tools.Monitor.Commands.Traces;
using Azure.Mcp.Tools.Monitor.Models.Traces;
using Azure.Mcp.Tools.Monitor.Services;
using Azure.Mcp.Tools.Monitor.Services.Traces;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.Tests.Traces;

public sealed class TraceAccountListCommandTests : SubscriptionCommandUnitTestsBase<TraceAccountListCommand, IMonitorTracesService>
{
    private const string KnownSubscription = "knownSubscription";

    [Theory]
    [InlineData($"--subscription {KnownSubscription}", true)]
    [InlineData($"--subscription {KnownSubscription} --resource-group my-rg", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            Service.ListAccountsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
                .Returns(new ResourceQueryResults<TraceAccountInfo>([], false));
        }

        var response = await ExecuteCommandAsync(args);

        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteAsync_ReturnsAccountsAndTruncationStatus(bool areResultsTruncated)
    {
        var accounts = new List<TraceAccountInfo>
        {
            new() { Name = "acct1", ResourceId = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.Monitor/accounts/acct1", ResourceGroup = "rg", Location = "eastus" },
            new() { Name = "acct2", ResourceId = "/subscriptions/s/resourceGroups/rg/providers/Microsoft.Monitor/accounts/acct2", ResourceGroup = "rg", Location = "westus", TraceDurationWindowInSeconds = 3600 }
        };
        Service.ListAccountsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<TraceAccountInfo>(accounts, areResultsTruncated));

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription}");
        var result = ValidateAndDeserializeResponse(response, MonitorJsonContext.Default.TraceAccountListCommandResult);

        Assert.Equal(2, result.Accounts.Count);
        Assert.Equal("acct1", result.Accounts[0].Name);
        Assert.Equal(3600, result.Accounts[1].TraceDurationWindowInSeconds);
        Assert.Equal(areResultsTruncated, result.AreResultsTruncated);
    }

    [Fact]
    public async Task ExecuteAsync_WithResourceGroup_ForwardsToService()
    {
        const string resourceGroup = "trace-rg";
        Service.ListAccountsAsync(Arg.Any<string>(), Arg.Is(resourceGroup), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new ResourceQueryResults<TraceAccountInfo>([], false));

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription} --resource-group {resourceGroup}");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        await Service.Received(1).ListAccountsAsync(Arg.Any<string>(), Arg.Is(resourceGroup), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        Service.ListAccountsAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        var response = await ExecuteCommandAsync($"--subscription {KnownSubscription}");

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("boom", response.Message);
    }
}
