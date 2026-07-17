// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data.Common;
using System.Runtime.CompilerServices;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Tests.Services.Support;
using Azure.ResourceManager.Resources;
using Microsoft.Mcp.Core.Commands;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Services;

public class PostgresServiceTests
{
    private readonly IResourceGroupService _resourceGroupService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly IEntraTokenProvider _entraTokenAuth;
    private readonly IDbProvider _dbProvider;
    private readonly PostgresService _postgresService;

    private string subscriptionId;
    private string resourceGroup;
    private string user;
    private string server;
    private string database;
    private string query;
    private string authType;

    public PostgresServiceTests()
    {
        _resourceGroupService = Substitute.For<IResourceGroupService>();
        _subscriptionService = Substitute.For<ISubscriptionService>();

        _tenantService = Substitute.For<ITenantService>();

        _entraTokenAuth = Substitute.For<IEntraTokenProvider>();
        _entraTokenAuth.GetEntraToken(Arg.Any<Azure.Core.TokenCredential>(), Arg.Any<CancellationToken>())
            .Returns(new Azure.Core.AccessToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _dbProvider = Substitute.For<IDbProvider>();
        _dbProvider.GetPostgresResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IPostgresResource>());
        _dbProvider.GetCommand(Arg.Any<string>(), Arg.Any<IPostgresResource>())
            .Returns(Substitute.For<NpgsqlCommand>());
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<DbDataReader>());

        _postgresService = new PostgresService(_resourceGroupService, _subscriptionService, _tenantService, _entraTokenAuth, _dbProvider);

        subscriptionId = "test-sub";
        resourceGroup = "test-rg";
        user = "test-user";
        server = "test-server";
        database = "test-db";
        query = "SELECT * FROM test-table;";
        authType = "MicrosoftEntra";
    }

    [Fact]
    public async Task ExecuteQueryAsync_InvalidCastException_Test()
    {
        // This test verifies that queries that returns unsupported data types return an exception
        // message that helps AI to understand the issue and fix the query.

        // Arrange
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FakeDbDataReader(
                [
                    ["row1", 1, new InvalidCastItem()],
                        ["row2", 2, new InvalidCastItem()],
                        ["row3", 3, new InvalidCastItem()]
                ],
                ["string", "integer", "unsupported"],
                [typeof(string), typeof(int), typeof(InvalidCastItem)]));

        // Act
        CommandValidationException exception = await Assert.ThrowsAsync<CommandValidationException>(async () =>
        {
            await _postgresService.ExecuteQueryAsync(authType, user, null, server, database, query, TestContext.Current.CancellationToken);
        });

        // Assert
        Assert.Contains("The PostgreSQL query failed because it returned one or more columns with non-standard data types (extension or user-defined) unsupported by the MCP agent", exception.Message);
    }

    [Fact]
    public async Task ExecuteQueryAsync_MixedDataTypes_Test()
    {
        // This test verifies that queries that return supported data types work as expected.

        // Arrange
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FakeDbDataReader(
                [
                    ["row1", 1,],
                        ["row2", 2,],
                        ["row3", 3,]
                ],
                ["string", "integer"],
                [typeof(string), typeof(int), typeof(InvalidCastItem)]));

        // Act
        List<string> rows = await _postgresService.ExecuteQueryAsync(authType, user, null, server, database, query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(4, rows.Count);
        Assert.Contains("string, integer", rows.ElementAt(0));
        Assert.Contains("row1, 1", rows.ElementAt(1));
        Assert.Contains("row2, 2", rows.ElementAt(2));
        Assert.Contains("row3, 3", rows.ElementAt(3));
    }

    [Fact]
    public async Task ExecuteQueryAsync_NoRows_Test()
    {
        // This test verifies that if no elements are found, only the header row is returned.

        // Arrange
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FakeDbDataReader(
                [],
                ["string", "integer"],
                [typeof(string), typeof(int), typeof(InvalidCastItem)]));

        // Act
        List<string> rows = await _postgresService.ExecuteQueryAsync(authType, user, null, server, database, query, TestContext.Current.CancellationToken);

        // Assert
        Assert.Single(rows);
        Assert.Contains("string, integer", rows.ElementAt(0));
    }

    [Fact]
    public async Task ListServersAsync_SubscriptionScope_ReturnsAllServers()
    {
        // Arrange — no resource group → subscription-wide path
        var expected = new List<string> { "server-a", "server-b" };
        var sut = new TestablePostgresService(
            _resourceGroupService, _subscriptionService, _tenantService,
            _entraTokenAuth, _dbProvider,
            subscriptionServers: expected,
            resourceGroupServers: []);

        // Act
        var result = await sut.ListServersAsync(subscriptionId, null, TestContext.Current.CancellationToken);

        // Assert — subscription service was called, RG service was not
        Assert.Equal(expected, result);
        await _subscriptionService.Received(1).GetSubscription(subscriptionId, cancellationToken: Arg.Any<CancellationToken>());
        await _resourceGroupService.DidNotReceive().GetResourceGroupResource(Arg.Any<string>(), Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListServersAsync_ResourceGroupScope_ReturnsFilteredServers()
    {
        // Arrange — resource group provided → RG-scoped path
        var expected = new List<string> { "server-rg1" };
        _resourceGroupService
            .GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Substitute.For<ResourceGroupResource>());

        var sut = new TestablePostgresService(
            _resourceGroupService, _subscriptionService, _tenantService,
            _entraTokenAuth, _dbProvider,
            subscriptionServers: [],
            resourceGroupServers: expected);

        // Act
        var result = await sut.ListServersAsync(subscriptionId, resourceGroup, TestContext.Current.CancellationToken);

        // Assert — RG service was called, subscription service was not
        Assert.Equal(expected, result);
        await _resourceGroupService.Received(1).GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: Arg.Any<CancellationToken>());
        await _subscriptionService.DidNotReceive().GetSubscription(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListServersAsync_ResourceGroupScope_ThrowsWhenRgNotFound()
    {
        // Arrange — RG service returns null (group does not exist)
        _resourceGroupService
            .GetResourceGroupResource(subscriptionId, resourceGroup, cancellationToken: Arg.Any<CancellationToken>())
            .Returns((ResourceGroupResource?)null);

        var sut = new TestablePostgresService(
            _resourceGroupService, _subscriptionService, _tenantService,
            _entraTokenAuth, _dbProvider,
            subscriptionServers: [],
            resourceGroupServers: []);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => sut.ListServersAsync(subscriptionId, resourceGroup, TestContext.Current.CancellationToken));
        Assert.Contains(resourceGroup, ex.Message);
    }

    [Fact]
    public async Task ListTablesAsync_UsesParameterizedSchemaQuery_AndDefaultsAreNotInterpolated()
    {
        // Verify the table listing query is parameterized on @schema (no string interpolation),
        // orders results deterministically, and actually binds the schema parameter value.
        string? capturedQuery = null;
        var command = Substitute.For<NpgsqlCommand>();
        _dbProvider.GetCommand(Arg.Do<string>(q => capturedQuery = q), Arg.Any<IPostgresResource>())
            .Returns(command);

        await _postgresService.ListTablesAsync(authType, user, null, server, database, "analytics", TestContext.Current.CancellationToken);

        Assert.NotNull(capturedQuery);
        Assert.Contains("@schema", capturedQuery);
        Assert.Contains("ORDER BY table_name", capturedQuery);
        Assert.DoesNotContain("'public'", capturedQuery);
        Assert.DoesNotContain("analytics", capturedQuery);

        Assert.Equal(2, command.Parameters.Count);
        Assert.Equal("schema", command.Parameters[0].ParameterName);
        Assert.Equal("analytics", command.Parameters[0].Value);
        Assert.Equal("maxResults", command.Parameters[1].ParameterName);
        Assert.Equal(PostgresService.MaxRowCount + 1, command.Parameters[1].Value);
    }

    [Fact]
    public async Task GetServerConfigAsync_ForwardsTenantAndRetryPolicy()
    {
        // Arrange
        var tenant = "tenant123";
        var retryPolicy = new Microsoft.Mcp.Core.Options.RetryPolicyOptions();
        _resourceGroupService
            .GetResourceGroupResource(subscriptionId, resourceGroup, tenant, retryPolicy, Arg.Any<CancellationToken>())
            .Returns((ResourceGroupResource?)null);

        // Act & Assert — resource group lookup happens first; null triggers the exception
        await Assert.ThrowsAsync<Exception>(() =>
            _postgresService.GetServerConfigAsync(subscriptionId, resourceGroup, user, server, tenant, retryPolicy, TestContext.Current.CancellationToken));

        await _resourceGroupService.Received(1)
            .GetResourceGroupResource(subscriptionId, resourceGroup, tenant, retryPolicy, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetServerParameterAsync_ForwardsTenantAndRetryPolicy()
    {
        // Arrange
        var tenant = "tenant123";
        var retryPolicy = new Microsoft.Mcp.Core.Options.RetryPolicyOptions();
        _resourceGroupService
            .GetResourceGroupResource(subscriptionId, resourceGroup, tenant, retryPolicy, Arg.Any<CancellationToken>())
            .Returns((ResourceGroupResource?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _postgresService.GetServerParameterAsync(subscriptionId, resourceGroup, user, server, "param123", tenant, retryPolicy, TestContext.Current.CancellationToken));

        await _resourceGroupService.Received(1)
            .GetResourceGroupResource(subscriptionId, resourceGroup, tenant, retryPolicy, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SetServerParameterAsync_ForwardsTenantAndRetryPolicy()
    {
        // Arrange
        var tenant = "tenant123";
        var retryPolicy = new Microsoft.Mcp.Core.Options.RetryPolicyOptions();
        _resourceGroupService
            .GetResourceGroupResource(subscriptionId, resourceGroup, tenant, retryPolicy, Arg.Any<CancellationToken>())
            .Returns((ResourceGroupResource?)null);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() =>
            _postgresService.SetServerParameterAsync(subscriptionId, resourceGroup, user, server, "param123", "value123", tenant, retryPolicy, TestContext.Current.CancellationToken));

        await _resourceGroupService.Received(1)
            .GetResourceGroupResource(subscriptionId, resourceGroup, tenant, retryPolicy, Arg.Any<CancellationToken>());
    }
}

/// <summary>
/// Subclass that replaces the un-mockable ARM SDK extension-method calls with
/// in-memory sequences, isolating <see cref="PostgresService.ListServersAsync"/> logic.
/// </summary>
internal sealed class TestablePostgresService(
    IResourceGroupService resourceGroupService,
    ISubscriptionService subscriptionService,
    ITenantService tenantService,
    IEntraTokenProvider entraTokenAuth,
    IDbProvider dbProvider,
    IEnumerable<string> subscriptionServers,
    IEnumerable<string> resourceGroupServers)
    : PostgresService(resourceGroupService, subscriptionService, tenantService, entraTokenAuth, dbProvider)
{
    protected override async IAsyncEnumerable<string> ListSubscriptionServerNamesAsync(
        SubscriptionResource subscription,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        foreach (var name in subscriptionServers)
            yield return name;
    }

    protected override async IAsyncEnumerable<string> ListResourceGroupServerNamesAsync(
        ResourceGroupResource resourceGroup,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        foreach (var name in resourceGroupServers)
            yield return name;
    }
}
