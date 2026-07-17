// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Tests.Services.Support;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Services;

public class PostgresServiceRowLimitTests
{
    private const int MaxRowCount = PostgresService.MaxRowCount;

    private readonly IResourceGroupService _resourceGroupService = Substitute.For<IResourceGroupService>();
    private readonly ISubscriptionService _subscriptionService = Substitute.For<ISubscriptionService>();
    private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
    private readonly IEntraTokenProvider _entraTokenAuth = Substitute.For<IEntraTokenProvider>();
    private readonly IDbProvider _dbProvider = Substitute.For<IDbProvider>();
    private readonly PostgresService _postgresService;

    private const string SubscriptionId = "test-sub";
    private const string ResourceGroup = "test-rg";
    private const string User = "test-user";
    private const string Server = "test-server";
    private const string Database = "test-db";
    private const string Schema = "public";
    private const string AuthType = "MicrosoftEntra";

    public PostgresServiceRowLimitTests()
    {
        _entraTokenAuth.GetEntraToken(Arg.Any<Azure.Core.TokenCredential>(), Arg.Any<CancellationToken>())
            .Returns(new Azure.Core.AccessToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _dbProvider.GetPostgresResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IPostgresResource>());
        _dbProvider.GetCommand(Arg.Any<string>(), Arg.Any<IPostgresResource>())
            .Returns(Substitute.For<NpgsqlCommand>());

        _postgresService = new PostgresService(_resourceGroupService, _subscriptionService, _tenantService, _entraTokenAuth, _dbProvider);
    }

    private void StubReader(int rowCount, string columnName)
    {
        var rows = Enumerable.Range(0, rowCount).Select(i => new[] { $"item{i:D5}" }).ToArray();
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(new FakeDbDataReader(rows, [columnName]));
    }

    [Fact]
    public async Task ListDatabasesAsync_UnderCap_ReturnsAllAndNotTruncated()
    {
        StubReader(rowCount: 3, columnName: "datname");

        var result = await _postgresService.ListDatabasesAsync(
            AuthType, User, null, Server, TestContext.Current.CancellationToken);

        Assert.Equal(3, result.Databases.Count);
        Assert.False(result.IsTruncated);
    }

    [Fact]
    public async Task ListDatabasesAsync_AtCap_ReturnsCapRowsAndNotTruncatedWhenNoExtra()
    {
        // Reader returns exactly MaxRowCount rows — boundary case, nothing beyond the cap.
        StubReader(rowCount: MaxRowCount, columnName: "datname");

        var result = await _postgresService.ListDatabasesAsync(
            AuthType, User, null, Server, TestContext.Current.CancellationToken);

        Assert.Equal(MaxRowCount, result.Databases.Count);
        Assert.False(result.IsTruncated);
    }

    [Fact]
    public async Task ListDatabasesAsync_OverCap_ReturnsCapRowsAndIsTruncated()
    {
        // Reader returns MaxRowCount + 1 (the cap+1 LIMIT in production); detect truncation via the extra read.
        StubReader(rowCount: MaxRowCount + 1, columnName: "datname");

        var result = await _postgresService.ListDatabasesAsync(
            AuthType, User, null, Server, TestContext.Current.CancellationToken);

        Assert.Equal(MaxRowCount, result.Databases.Count);
        Assert.True(result.IsTruncated);
    }

    [Fact]
    public async Task ListTablesAsync_UnderCap_ReturnsAllAndNotTruncated()
    {
        StubReader(rowCount: 5, columnName: "table_name");

        var result = await _postgresService.ListTablesAsync(
            AuthType, User, null, Server, Database, Schema, TestContext.Current.CancellationToken);

        Assert.Equal(5, result.Tables.Count);
        Assert.False(result.IsTruncated);
    }

    [Fact]
    public async Task ListTablesAsync_AtCap_ReturnsCapRowsAndNotTruncatedWhenNoExtra()
    {
        // Reader returns exactly MaxRowCount rows — boundary case, nothing beyond the cap.
        StubReader(rowCount: MaxRowCount, columnName: "table_name");

        var result = await _postgresService.ListTablesAsync(
            AuthType, User, null, Server, Database, Schema, TestContext.Current.CancellationToken);

        Assert.Equal(MaxRowCount, result.Tables.Count);
        Assert.False(result.IsTruncated);
    }

    [Fact]
    public async Task ListTablesAsync_OverCap_ReturnsCapRowsAndIsTruncated()
    {
        // Reader returns MaxRowCount + 1 (the cap+1 LIMIT in production); detect truncation via the extra read.
        StubReader(rowCount: MaxRowCount + 1, columnName: "table_name");

        var result = await _postgresService.ListTablesAsync(
            AuthType, User, null, Server, Database, Schema, TestContext.Current.CancellationToken);

        Assert.Equal(MaxRowCount, result.Tables.Count);
        Assert.True(result.IsTruncated);
    }
}
