// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.MySql.Commands;
using Azure.Mcp.Tools.MySql.Commands.Database;
using Azure.Mcp.Tools.MySql.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.MySql.UnitTests.Database;

public class DatabaseQueryCommandTests : CommandUnitTestsBase<DatabaseQueryCommand, IMySqlService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsResults_WhenQuerySucceeds()
    {
        var expectedResults = new List<string> { "id, name", "1, John", "2, Jane" };
        Service.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db1", "SELECT * FROM users", Arg.Any<CancellationToken>()).Returns(expectedResults);

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--query", "SELECT * FROM users");

        var result = ValidateAndDeserializeResponse(response, MySqlJsonContext.Default.DatabaseQueryCommandResult);
        Assert.Equal(expectedResults, result.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsError_WhenQueryFails()
    {
        Service.ExecuteQueryAsync("sub123", "rg1", "user1", "server1", "db1", "INVALID SQL", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Syntax error"));

        var response = await ExecuteCommandAsync(
            "--subscription", "sub123",
            "--resource-group", "rg1",
            "--user", "user1",
            "--server", "server1",
            "--database", "db1",
            "--query", "INVALID SQL");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.Status);
        Assert.Contains("Syntax error", response.Message);
    }

    [Fact]
    public void Metadata_IsConfiguredCorrectly()
    {
        Assert.False(Command.Metadata.Destructive);
        Assert.True(Command.Metadata.ReadOnly);
    }
}
