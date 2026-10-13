// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Sql.Services;
using Azure.ResourceManager;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Sql.Tests.Services;

public class SqlServiceTests
{
    private const string SubscriptionName = "my-subscription";
    private const string SubscriptionId = "12345678-1234-1234-1234-123456789012";
    private const string ResolveSentinel = "SqlServiceTests: subscription name resolved via IAzureService";
    private const string ServerName = "server1";
    private const string ResourceGroup = "rg1";
    private const string DatabaseName = "db1";

    // Distinctive message thrown by the mocked subscription service so tests can prove
    // the service resolves the subscription via IAzureService (the #449/#453 fix)
    // instead of building a SubscriptionResource directly from the raw value.
    private const string SubscriptionResolvedMessage = "SqlServiceTests: subscription resolved via IAzureService";

    private readonly IAzureService _azureService;
    private readonly ILogger<SqlService> _logger;
    private readonly SqlService _service;

    public SqlServiceTests()
    {
        _azureService = Substitute.For<IAzureService>();
        _logger = Substitute.For<ILogger<SqlService>>();

        var cloudConfig = Substitute.For<IAzureCloudConfiguration>();
        cloudConfig.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        cloudConfig.AuthorityHost.Returns(new Uri("https://login.microsoftonline.com"));
        cloudConfig.ArmEnvironment.Returns(ArmEnvironment.AzurePublicCloud);
        _azureService.CloudConfiguration.Returns(cloudConfig);

        var credential = Substitute.For<TokenCredential>();
        _azureService.GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(credential));

        _azureService.GetSubscription(
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(SubscriptionResolvedMessage));

        _service = new SqlService(_azureService, _logger);
    }

    [Fact]
    public async Task GetServerAsync_ResolvesSubscriptionThroughAzureService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.GetServerAsync("server1", "rg", SubscriptionName, TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task ListServersAsync_ResolvesSubscriptionThroughAzureService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ListServersAsync("rg", SubscriptionName, TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task CreateServerAsync_ResolvesSubscriptionThroughAzureService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateServerAsync(
                "server1",
                "rg",
                SubscriptionName,
                "eastus",
                "admin",
                "P@ssw0rd!",
                null,
                null,
                TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    [Fact]
    public async Task RenameDatabaseAsync_ResolvesSubscriptionThroughAzureService()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RenameDatabaseAsync(
                "server1",
                "olddb",
                "newdb",
                "rg",
                SubscriptionName,
                TestContext.Current.CancellationToken));

        Assert.Equal(SubscriptionResolvedMessage, ex.Message);
        await AssertSubscriptionResolvedAsync();
    }

    private Task AssertSubscriptionResolvedAsync() =>
        _azureService.Received(1).GetSubscription(
            SubscriptionName,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>());

    [Fact]
    public async Task ListDatabasesAsync_WithSubscriptionName_ResolvesNameToId()
    {
        _azureService.IsSubscriptionId(SubscriptionName).Returns(false);
        _azureService.GetSubscriptionIdByName(SubscriptionName, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(ResolveSentinel));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ListDatabasesAsync(ServerName, ResourceGroup, SubscriptionName, TestContext.Current.CancellationToken));

        Assert.Equal(ResolveSentinel, exception.Message);
        await _azureService.Received(1).GetSubscriptionIdByName(
            SubscriptionName, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetElasticPoolsAsync_WithSubscriptionName_ResolvesNameToId()
    {
        _azureService.IsSubscriptionId(SubscriptionName).Returns(false);
        _azureService.GetSubscriptionIdByName(SubscriptionName, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(ResolveSentinel));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetElasticPoolsAsync(ServerName, ResourceGroup, SubscriptionName, TestContext.Current.CancellationToken));

        Assert.Equal(ResolveSentinel, exception.Message);
        await _azureService.Received(1).GetSubscriptionIdByName(
            SubscriptionName, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListDatabasesAsync_WithSubscriptionId_SkipsNameLookup()
    {
        _azureService.IsSubscriptionId(SubscriptionId).Returns(true);
        var canceled = new CancellationToken(canceled: true);

        try
        {
            await _service.ListDatabasesAsync(ServerName, ResourceGroup, SubscriptionId, canceled);
        }
        catch
        {
            // The ARM hierarchy call is expected to fail/cancel; we only assert resolution behavior.
        }

        await _azureService.DidNotReceive().GetSubscriptionIdByName(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetElasticPoolsAsync_WithSubscriptionId_SkipsNameLookup()
    {
        _azureService.IsSubscriptionId(SubscriptionId).Returns(true);
        var canceled = new CancellationToken(canceled: true);

        try
        {
            await _service.GetElasticPoolsAsync(ServerName, ResourceGroup, SubscriptionId, canceled);
        }
        catch
        {
            // The ARM hierarchy call is expected to fail/cancel; we only assert resolution behavior.
        }

        await _azureService.DidNotReceive().GetSubscriptionIdByName(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDatabaseAsync_WithSubscriptionName_ResolvesNameToId()
    {
        _azureService.IsSubscriptionId(SubscriptionName).Returns(false);
        _azureService.GetSubscriptionIdByName(SubscriptionName, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException(ResolveSentinel));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.GetDatabaseAsync(ServerName, DatabaseName, ResourceGroup, SubscriptionName, TestContext.Current.CancellationToken));

        Assert.Equal(ResolveSentinel, exception.Message);
        await _azureService.Received(1).GetSubscriptionIdByName(
            SubscriptionName, Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetDatabaseAsync_WithSubscriptionId_SkipsNameLookup()
    {
        _azureService.IsSubscriptionId(SubscriptionId).Returns(true);
        var canceled = new CancellationToken(canceled: true);

        try
        {
            await _service.GetDatabaseAsync(ServerName, DatabaseName, ResourceGroup, SubscriptionId, canceled);
        }
        catch
        {
            // The ARM hierarchy call is expected to fail/cancel; we only assert resolution behavior.
        }

        await _azureService.DidNotReceive().GetSubscriptionIdByName(
            Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

}
