// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Data.Common;
using System.Security;
using Azure.Core;
using Azure.Mcp.Core.Services.Azure;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Providers;
using Azure.Mcp.Tools.Postgres.Services;
using Azure.Mcp.Tools.Postgres.Tests.Services.Support;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using Npgsql;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Services;

/// <summary>
/// Tests to verify that PostgreSQL server validation rejects non-Azure hostnames,
/// preventing credential exfiltration to attacker-controlled servers.
/// </summary>
public class PostgresServiceServerNameValidationTests
{
    private readonly IAzureService _azureService;
    private readonly IAzureCloudConfiguration _cloudConfiguration;
    private readonly IEntraTokenProvider _entraTokenAuth;
    private readonly IDbProvider _dbProvider;
    private readonly PostgresService _postgresService;
    private string? _capturedConnectionString;

    public PostgresServiceServerNameValidationTests()
    {
        _azureService = Substitute.For<IAzureService>();
        _cloudConfiguration = _azureService.ConfigureCloud(ArmEnvironment.AzurePublicCloud);

        _entraTokenAuth = Substitute.For<IEntraTokenProvider>();
        _entraTokenAuth.GetEntraToken(Arg.Any<TokenCredential>(), Arg.Any<CancellationToken>())
            .Returns(new AccessToken("fake-token", DateTime.UtcNow.AddHours(1)));

        _dbProvider = Substitute.For<IDbProvider>();
        _dbProvider.GetPostgresResource(Arg.Do<string>(cs => _capturedConnectionString = cs), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<IPostgresResource>());
        _dbProvider.GetCommand(Arg.Any<string>(), Arg.Any<IPostgresResource>())
            .Returns(Substitute.For<NpgsqlCommand>());
        _dbProvider.ExecuteReaderAsync(Arg.Any<NpgsqlCommand>(), Arg.Any<CancellationToken>())
            .Returns(Substitute.For<DbDataReader>());

        _postgresService = new PostgresService(_azureService, _entraTokenAuth, _dbProvider);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    [InlineData("fake.postgres.database.azure.com.attacker.com")]
    [InlineData("myserver.postgres.database.azure.com.evil.org")]
    [InlineData("postgres.database.azure.com.attacker.net")]
    [InlineData("malicious.host")]
    public async Task ExecuteQueryAsync_WithUnauthorizedServerFQDN_ThrowsSecurityException(string maliciousServer)
    {
        SecurityException exception = await Assert.ThrowsAsync<SecurityException>(() =>
            _postgresService.ExecuteQueryAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer, "testdb", "SELECT 1",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid postgres domain", exception.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _entraTokenAuth.DidNotReceive()
            .GetEntraToken(Arg.Any<TokenCredential>(), Arg.Any<CancellationToken>());
        await _dbProvider.DidNotReceive()
            .GetPostgresResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task ListDatabasesAsync_WithUnauthorizedServerFQDN_ThrowsSecurityException(string maliciousServer)
    {
        SecurityException exception = await Assert.ThrowsAsync<SecurityException>(() =>
            _postgresService.ListDatabasesAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer,
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid postgres domain", exception.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task ListTablesAsync_WithUnauthorizedServerFQDN_ThrowsSecurityException(string maliciousServer)
    {
        SecurityException exception = await Assert.ThrowsAsync<SecurityException>(() =>
            _postgresService.ListTablesAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer, "testdb", "public",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid postgres domain", exception.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task GetTableSchemaAsync_WithUnauthorizedServerFQDN_ThrowsSecurityException(string maliciousServer)
    {
        SecurityException exception = await Assert.ThrowsAsync<SecurityException>(() =>
            _postgresService.GetTableSchemaAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                maliciousServer, "testdb", "test_table",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid postgres domain", exception.Message);
    }

    [Theory]
    [InlineData("attacker.com,myserver.postgres.database.azure.com")]
    [InlineData("attacker.com@myserver.postgres.database.azure.com")]
    [InlineData("myserver.postgres.database.azure.com/path")]
    [InlineData("myserver.postgres.database.azure.com?host=attacker.com")]
    [InlineData("myserver.postgres.database.azure.com#attacker.com")]
    [InlineData("myserver.postgres.database.azure.com:5432")]
    [InlineData("https://myserver.postgres.database.azure.com")]
    public async Task ExecuteQueryAsync_WithInvalidServerSyntax_ThrowsArgumentException(string server)
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postgresService.ExecuteQueryAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                server, "testdb", "SELECT 1",
                TestContext.Current.CancellationToken));

        Assert.Contains(
            "short Azure Database for PostgreSQL server name or a fully qualified Azure Database for PostgreSQL hostname",
            exception.Message);
        await _azureService.DidNotReceive()
            .GetTokenCredentialAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>());
        await _dbProvider.DidNotReceive()
            .GetPostgresResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteQueryAsync_WithPostgresSuffixRoot_ThrowsArgumentException()
    {
        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _postgresService.ExecuteQueryAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                "postgres.database.azure.com", "testdb", "SELECT 1",
                TestContext.Current.CancellationToken));

        Assert.Contains("does not include a server label", exception.Message);
    }

    [Theory]
    [InlineData("myserver.postgres.database.azure.com")]
    [InlineData("MyServer.Postgres.Database.Azure.Com")]
    public async Task ExecuteQueryAsync_WithValidAzureServerFQDN_DoesNotThrow(string validServer)
    {
        // Should not throw - valid Azure PostgreSQL FQDNs are accepted
        await _postgresService.ExecuteQueryAsync(
            AuthTypes.MicrosoftEntra, "test-user", null,
            validServer, "testdb", "SELECT 1",
            TestContext.Current.CancellationToken);

        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);
        Assert.Equal(validServer, parsed.Host);
    }

    [Theory]
    [InlineData("Public", "myserver.postgres.database.azure.com")]
    [InlineData("China", "myserver.postgres.database.chinacloudapi.cn")]
    [InlineData("Government", "myserver.postgres.database.usgovcloudapi.net")]
    public async Task ExecuteQueryAsync_WithShortServerName_AppendsConfiguredCloudSuffix(
        string cloud,
        string expectedHost)
    {
        ConfigureCloud(cloud);

        await _postgresService.ExecuteQueryAsync(
            AuthTypes.MicrosoftEntra, "test-user", null,
            "myserver", "testdb", "SELECT 1",
            TestContext.Current.CancellationToken);

        Assert.NotNull(_capturedConnectionString);
        var parsed = new NpgsqlConnectionStringBuilder(_capturedConnectionString!);
        Assert.Equal(expectedHost, parsed.Host);
    }

    [Theory]
    [InlineData("myserver.postgres.database.usgovcloudapi.net")]
    [InlineData("myserver.postgres.database.chinacloudapi.cn")]
    public async Task ExecuteQueryAsync_WithEndpointFromDifferentCloud_ThrowsSecurityException(string server)
    {
        SecurityException exception = await Assert.ThrowsAsync<SecurityException>(() =>
            _postgresService.ExecuteQueryAsync(
                AuthTypes.MicrosoftEntra, "test-user", null,
                server, "testdb", "SELECT 1",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid postgres domain", exception.Message);
        await _dbProvider.DidNotReceive()
            .GetPostgresResource(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    private void ConfigureCloud(string cloud)
    {
        ArmEnvironment armEnvironment = cloud switch
        {
            "Public" => ArmEnvironment.AzurePublicCloud,
            "China" => ArmEnvironment.AzureChina,
            "Government" => ArmEnvironment.AzureGovernment,
            _ => throw new ArgumentOutOfRangeException(nameof(cloud))
        };

        _cloudConfiguration.ArmEnvironment.Returns(armEnvironment);
    }
}
