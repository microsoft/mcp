// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.ResourceGroup;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Services;

/// <summary>
/// Tests to verify that NormalizeServerName rejects non-Azure hostnames,
/// preventing credential exfiltration to attacker-controlled servers.
/// </summary>
public class MySqlServiceServerNameValidationTests
{
    private readonly MySqlService _mysqlService;

    public MySqlServiceServerNameValidationTests()
    {
        var resourceGroupService = Substitute.For<IResourceGroupService>();
        var tenantService = Substitute.For<ITenantService>();
        var logger = Substitute.For<ILogger<MySqlService>>();

        _mysqlService = new MySqlService(resourceGroupService, tenantService, logger);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    [InlineData("fake.mysql.database.azure.com.attacker.com")]
    [InlineData("myserver.mysql.database.azure.com.evil.org")]
    [InlineData("mysql.database.azure.com.attacker.net")]
    [InlineData("malicious.host")]
    public async Task ListDatabasesAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        // NormalizeServerName runs before token acquisition, so ArgumentException
        // is thrown before any credential or network operation.
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mysqlService.ListDatabasesAsync(
                "test-sub", "test-rg", "test-user",
                maliciousServer,
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for MySQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task ExecuteQueryAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mysqlService.ExecuteQueryAsync(
                "test-sub", "test-rg", "test-user",
                maliciousServer, "testdb", "SELECT 1",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for MySQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task GetTablesAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mysqlService.GetTablesAsync(
                "test-sub", "test-rg", "test-user",
                maliciousServer, "testdb",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for MySQL hostname", ex.Message);
    }

    [Theory]
    [InlineData("attacker.com")]
    [InlineData("evil.example.org")]
    public async Task GetTableSchemaAsync_WithNonAzureServerFQDN_ThrowsArgumentException(string maliciousServer)
    {
        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            _mysqlService.GetTableSchemaAsync(
                "test-sub", "test-rg", "test-user",
                maliciousServer, "testdb", "test_table",
                TestContext.Current.CancellationToken));

        Assert.Contains("not a valid Azure Database for MySQL hostname", ex.Message);
    }
}
