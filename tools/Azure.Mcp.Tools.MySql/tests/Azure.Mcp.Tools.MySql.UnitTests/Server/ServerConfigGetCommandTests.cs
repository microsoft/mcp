// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Server;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Server;

public class ServerConfigGetCommandTests : CommandUnitTestsBase<ServerConfigGetCommand, IMySqlService>
{
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
        }, MySqlJsonContext.Default.ServerConfigGetResult);

        Service.GetServerConfigAsync("sub123", "rg1", "user1", "test-server", Arg.Any<CancellationToken>()).Returns(expectedConfig);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.ServerConfigGetCommandResult);
        Assert.Equal(expectedConfig, result.Configuration);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenServiceThrows()
    {
        Service.GetServerConfigAsync("sub123", "rg1", "user1", "test-server", Arg.Any<CancellationToken>())
            .ThrowsAsync(new UnauthorizedAccessException("Access denied"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "test-server");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Access denied", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }
}
