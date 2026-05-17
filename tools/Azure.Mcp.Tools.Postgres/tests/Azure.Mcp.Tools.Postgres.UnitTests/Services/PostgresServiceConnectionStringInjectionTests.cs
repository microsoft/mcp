// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data.Common;
using Azure.Core;
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
/// Regression tests to verify that connection string injection via user-controlled
/// parameters (database, server, user) is prevented by using NpgsqlConnectionStringBuilder.
/// </summary>
public class PostgresServiceConnectionStringInjectionTests
{
    private readonly IDbProvider _dbProvider;
    private readonly PostgresService _postgresService;
    private string? _capturedConnectionString;

    public PostgresServiceConnectionStringInjectionTests()
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

    private static void AssertConnectionStringNotInjected(string connectionString, string expectedHost, string injectedHost)
    {
        // Parse the resulting connection string using the builder to inspect individual values
        var parsed = new NpgsqlConnectionStringBuilder(connectionString);

        // The Host must be the legitimate server, not the injected value
        Assert.Equal(expectedHost, parsed.Host);
        Assert.DoesNotContain(injectedHost, parsed.Host, StringComparison.OrdinalIgnoreCase);

        // The malicious input is stored as a literal database name (harmless)
        // rather than being interpreted as separate connection parameters
    }

    [Theory]
    [InlineData("postgres;Host=attacker.com;SSL Mode=Disable", "attacker.com")]
    [InlineData("mydb;Host=evil.example.org", "evil.example.org")]
    [InlineData("testdb;Host=malicious.host;Timeout=1", "malicious.host")]
    public async Task ExecuteQueryAsync_WithInjectedDatabase_DoesNotOverrideHost(string maliciousDatabase, string injectedHost)
    {
        // Act
        await _postgresService.ExecuteQueryAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            "legitimate-server", maliciousDatabase, "SELECT 1",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_capturedConnectionString);
        var expectedHost = "legitimate-server.postgres.database.azure.com";
        AssertConnectionStringNotInjected(_capturedConnectionString!, expectedHost, injectedHost);
    }

    [Theory]
    [InlineData("postgres;Host=attacker.com;SSL Mode=Disable", "attacker.com")]
    [InlineData("mydb;Host=evil.example.org", "evil.example.org")]
    public async Task ListTablesAsync_WithInjectedDatabase_DoesNotOverrideHost(string maliciousDatabase, string injectedHost)
    {
        // Act
        await _postgresService.ListTablesAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            "legitimate-server", maliciousDatabase,
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_capturedConnectionString);
        var expectedHost = "legitimate-server.postgres.database.azure.com";
        AssertConnectionStringNotInjected(_capturedConnectionString!, expectedHost, injectedHost);
    }

    [Theory]
    [InlineData("postgres;Host=attacker.com;SSL Mode=Disable", "attacker.com")]
    [InlineData("mydb;Host=evil.example.org", "evil.example.org")]
    public async Task GetTableSchemaAsync_WithInjectedDatabase_DoesNotOverrideHost(string maliciousDatabase, string injectedHost)
    {
        // Act
        await _postgresService.GetTableSchemaAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            "legitimate-server", maliciousDatabase, "some_table",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_capturedConnectionString);
        var expectedHost = "legitimate-server.postgres.database.azure.com";
        AssertConnectionStringNotInjected(_capturedConnectionString!, expectedHost, injectedHost);
    }

    [Fact]
    public async Task ExecuteQueryAsync_WithSemicolonInDatabase_PreservesOriginalHost()
    {
        // Arrange
        const string legitimateServer = "my-server";
        const string maliciousDatabase = "mydb;Host=attacker.com;SSL Mode=Disable;Trust Server Certificate=true";

        // Act
        await _postgresService.ExecuteQueryAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            legitimateServer, maliciousDatabase, "SELECT 1",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);

        // Host must be the normalized legitimate server, not the attacker's host
        Assert.Contains("my-server", parsed.Host);
        Assert.DoesNotContain("attacker.com", parsed.Host, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteQueryAsync_WithSslDowngradeInDatabase_DoesNotDisableSsl()
    {
        // Arrange — attacker tries to inject SSL Mode=Disable via the database parameter
        const string maliciousDatabase = "postgres;SSL Mode=Disable";

        // Act
        await _postgresService.ExecuteQueryAsync(
            "test-sub", "test-rg", AuthTypes.MicrosoftEntra, "test-user", null,
            "safe-server", maliciousDatabase, "SELECT 1",
            TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);

        // SSL Mode should not be Disable — the builder escapes the value in the Database field
        Assert.NotEqual(SslMode.Disable, parsed.SslMode);
    }
}
