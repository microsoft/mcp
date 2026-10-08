// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.MySql.Services;
using Azure.ResourceManager;
using Azure.ResourceManager.MySql.FlexibleServers;
using Azure.ResourceManager.MySql.FlexibleServers.Mocking;
using Azure.ResourceManager.MySql.FlexibleServers.Models;
using Azure.ResourceManager.Resources;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.Tests.Services;

public class MySqlServiceTests
{
    private readonly IAzureService _azureService;
    private readonly MySqlService _mysqlService;

    public MySqlServiceTests()
    {
        _azureService = Substitute.For<IAzureService>();

        _mysqlService = new MySqlService(_azureService);
    }

    [Fact]
    public void Constructor_WithNullAzureService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MySqlService(null!));
    }

    [Fact]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        var service = new MySqlService(_azureService);
        Assert.NotNull(service);
    }

    [Fact]
    public async Task ListServersAsync_WhenAzureServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Resource group not found");
        _azureService.GetResourceGroupResource("sub123", "rg1", Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersAsync("sub123", "rg1", TestContext.Current.CancellationToken));

        Assert.Equal(exception, thrownException);
    }

    [Fact]
    public async Task ListServersInSubscriptionAsync_WhenAzureServiceThrows_RethrowsException()
    {
        var exception = new InvalidOperationException("Subscription not found");
        _azureService.GetSubscription("sub123", Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        var thrownException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _mysqlService.ListServersInSubscriptionAsync("sub123", TestContext.Current.CancellationToken));

        Assert.Equal(exception, thrownException);
    }

    [Fact]
    public async Task ListServersAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.ListServersAsync("sub123", "missing-rg", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task GetServerConfigAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.GetServerConfigAsync("sub123", "missing-rg", "some-server", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Fact]
    public async Task GetServerParameterAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.GetServerParameterAsync("sub123", "missing-rg", "some-server", "some-param", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }

    [Theory]
    [InlineData("system-default", "10")]
    [InlineData("user-override", "30")]
    public async Task SetServerParameterAsync_SetsValueAndUserOverrideSource(string source, string currentValue)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var resourceGroup = Substitute.For<ResourceGroupResource>();
        var mysqlResourceGroup = Substitute.For<MockableMySqlFlexibleServersResourceGroupResource>();
        var server = Substitute.For<MySqlFlexibleServerResource>();
        var configuration = Substitute.For<MySqlFlexibleServerConfigurationResource>();
        var configurations = Substitute.For<MySqlFlexibleServerConfigurationCollection>();
        var updatedConfiguration = Substitute.For<MySqlFlexibleServerConfigurationResource>();
        var operation = Substitute.For<ArmOperation<MySqlFlexibleServerConfigurationResource>>();
        var response = Substitute.For<Response>();

        _azureService.GetResourceGroupResource("sub123", "rg1", null, cancellationToken)
            .Returns(resourceGroup);
        resourceGroup.GetCachedClient(Arg.Any<Func<ArmClient, MockableMySqlFlexibleServersResourceGroupResource>>())
            .Returns(mysqlResourceGroup);
        mysqlResourceGroup.GetMySqlFlexibleServerAsync("test-server", cancellationToken)
            .Returns(Response.FromValue(server, response));
        configuration.Data.Returns(new MySqlFlexibleServerConfigurationData
        {
            Value = currentValue,
            Source = new MySqlFlexibleServerConfigurationSource(source)
        });
        server.GetMySqlFlexibleServerConfigurationAsync("connect_timeout", cancellationToken)
            .Returns(Response.FromValue(configuration, response));
        server.GetMySqlFlexibleServerConfigurations().Returns(configurations);
        updatedConfiguration.Data.Returns(new MySqlFlexibleServerConfigurationData
        {
            Value = "20",
            Source = MySqlFlexibleServerConfigurationSource.UserOverride
        });
        operation.Value.Returns(updatedConfiguration);
        operation.HasCompleted.Returns(true);
        configurations.CreateOrUpdateAsync(WaitUntil.Started, "connect_timeout",
            Arg.Any<MySqlFlexibleServerConfigurationData>(), cancellationToken)
            .Returns(operation);

        var result = await _mysqlService.SetServerParameterAsync(
            "sub123", "rg1", "test-server", "connect_timeout", "20", cancellationToken);

        Assert.Equal("20", result);
        await configurations.Received(1).CreateOrUpdateAsync(WaitUntil.Started, "connect_timeout",
            Arg.Is<MySqlFlexibleServerConfigurationData>(data =>
                data.Value == "20" && data.Source == MySqlFlexibleServerConfigurationSource.UserOverride),
            cancellationToken);
    }

    [Fact]
    public async Task SetServerParameterAsync_WhenResourceGroupNotFound_ThrowsKeyNotFoundException()
    {
        _azureService.GetResourceGroupResource("sub123", "missing-rg", Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Azure.ResourceManager.Resources.ResourceGroupResource?>(null));

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _mysqlService.SetServerParameterAsync("sub123", "missing-rg", "some-server", "some-param", "some-value", TestContext.Current.CancellationToken));

        Assert.Contains("missing-rg", ex.Message);
    }
}
