// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data.Common;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure.Authentication;
using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Postgres.Auth;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.UnitTests.Services;

/// <summary>
/// Tests to verify that NormalizeServerName rejects non-Azure hostnames,
/// preventing credential exfiltration to attacker-controlled servers.
/// </summary>
public class PostgresServiceServerNameValidationTests
{
    private readonly IDbProvider _dbProvider;
    private readonly PostgresService _postgresService;
    private string? _capturedConnectionString;

    public PostgresServiceServerNameValidationTests()
    {
        var resourceGroupService = Substitute.For<IResourceGroupService>();
        var tenantService = Substitute.For<ITenantService>();
        var subscriptionService = Substitute.For<ISubscriptionService>();

        var entraTokenAuth = Substitute.For<IEntraTokenProvider>();
        entraTokenAuth.GetEntraToken(Arg.Any<TokenCredential>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _dbProvider = Substitute.For<IDbProvider>();
        _dbProvider.GetPostgresResource(Arg.Do<string>(cs => _capturedConnectionString = cs), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IPostgresResource>());
        _dbProvider.GetCommand(Arg.Any<string>(), Arg.Any<IPostgresResource>())
            .Returns(Substitute.For<NpgsqlCommand>());
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<DbDataReader>());

        _postgresService = new PostgresService(resourceGroupService, subscriptionService, tenantService, entraTokenAuth, _dbProvider);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    [InlineData("fake.postgres.database.azure.com.attacker.com")]
    [InlineData("myserver.postgres.database.azure.com.evil.org")]
    [InlineData("postgres.database.azure.com.attacker.net")]
    [InlineData("malicious.host")]
    public async Task ExecuteQueryAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postgresService.ExecuteQueryAsync(
                "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer, "testdb", "SELECT 1",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for PostgreSQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task ListDatabasesAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postgresService.ListDatabasesAsync(
                "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer,
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for PostgreSQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task ListTablesAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postgresService.ListTablesAsync(
                "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer, "testdb",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for PostgreSQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task GetTableSchemaAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postgresService.GetTableSchemaAsync(
                "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer, "testdb", "test_table",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for PostgreSQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("myserver.postgres.database.azure.com")]
    [InlineData("myserver.postgres.database.usgovcloudapi.net")]
    [InlineData("myserver.postgres.database.chinacloudapi.cn")]
    [InlineData("MyServer.Postgres.Database.Azure.Com")]
    public async Task ExecuteQueryAsync_WithValidAzureServerFQDN_DoesNotThrow(string validServer)
    {
        // Should not throw - valid Azure PostgreSQL FQDNs are accepted
        await _postgresService.ExecuteQueryAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            validServer, "testdb", "SELECT 1",
            TestContext.Current.CancellationToken);

        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);
        Assert.Equal(validServer, parsed.Host);
    }

    [Fact]
    public async Task ExecuteQueryAsync_WithShortServerName_AppendsSuffix()
    {
        // Short names (no dot) get the suffix appended automatically
        await _postgresService.ExecuteQueryAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            "myserver", "testdb", "SELECT 1",
            TestContext.Current.CancellationToken);

        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);
        Assert.Equal("myserver.postgres.database.azure.com", parsed.Host);
    }
}
