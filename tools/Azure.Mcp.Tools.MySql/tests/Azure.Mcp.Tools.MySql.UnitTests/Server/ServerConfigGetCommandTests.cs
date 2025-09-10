// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Server;

public class ServerConfigGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IMySqlService _mysqlService;
    private readonly ILogger<ServerConfigGetCommand> _logger;

    public ServerConfigGetCommandTests()
    {
        _mysqlService = Substitute.For<IMySqlService>();
        _logger = Substitute.For<ILogger<ServerConfigGetCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_mysqlService);

        _serviceProvider = collection.BuildServiceProvider();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsConfiguration_WhenSuccessful()
    {
        var expectedConfig = JsonSerializer.Serialize(new
        {
            ServerName = "test-server",
            Location = "East US",
            Version = "8.0.21",
            SKU = "Standard_B1ms",
            StorageSizeGB = 20,
            BackupRetentionDays = 7,
            GeoRedundantBackup = "Disabled"
        }, new JsonSerializerOptions { WriteIndented = true });

        _mysqlService.GetServerConfigAsync("sub123", "rg1", "user1", "test-server").Returns(expectedConfig);

        var command = new ServerConfigGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ServerConfigGetCommand.ServerConfigGetCommandResult>(json);
        Assert.NotNull(result);
        Assert.Equal(expectedConfig, result.Configuration);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        _mysqlService.GetServerConfigAsync("sub123", "rg1", "user1", "test-server")
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var command = new ServerConfigGetCommand(_logger);
        var args = command.GetCommand().Parse([
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server"
        ]);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        var command = new ServerConfigGetCommand(_logger);

        Assert.Equal("config", command.Name);
        Assert.Equal("Retrieves comprehensive configuration details for the specified Azure Database for MySQL Flexible Server instance. This command provides insights into server settings, performance parameters, security configurations, and operational characteristics essential for database administration and optimization. Returns configuration data in JSON format including ServerName, Location, Version, SKU, StorageSizeGB, BackupRetentionDays, and GeoRedundantBackup properties.", command.Description);
        Assert.Equal("Get MySQL Server Configuration", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
    }

    private class ServerConfigResult
    {
        [JsonPropertyName("Configuration")]
        public string Configuration { get; set; } = string.Empty;
    }
}
