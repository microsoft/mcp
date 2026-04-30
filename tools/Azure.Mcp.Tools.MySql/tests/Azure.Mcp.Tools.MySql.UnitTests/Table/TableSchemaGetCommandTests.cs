// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Table;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Table;

public class TableSchemaGetCommandTests : CommandUnitTestsBase<TableSchemaGetCommand, IMySqlService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenSuccessful()
    {
        var expectedSchema = new List<string> { "id INT PRIMARY KEY", "name VARCHAR(100) NOT NULL", "email VARCHAR(255)" };
        Service.GetTableSchemaAsync("sub123", "rg1", "user1", "server1", "db1", "users", Arg.Any<CancellationToken>()).Returns(expectedSchema);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--table", "users");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.TableSchemaGetCommandResult);
        Assert.Equal(expectedSchema, result.Schema);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenTableNotFound()
    {
        Service.GetTableSchemaAsync("sub123", "rg1", "user1", "server1", "db1", "nonexistent", Arg.Any<CancellationToken>()).ThrowsAsync(new ArgumentException("Table not found"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--table", "nonexistent");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Table not found", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }
}
