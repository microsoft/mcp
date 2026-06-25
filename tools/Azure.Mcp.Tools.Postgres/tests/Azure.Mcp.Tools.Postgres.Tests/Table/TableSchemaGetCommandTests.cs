// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Postgres.Commands;
using Azure.Mcp.Tools.Postgres.Commands.Table;
using Azure.Mcp.Tools.Postgres.Options;
using Azure.Mcp.Tools.Postgres.Services;
using Microsoft.Mcp.Core.TestUtilities;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Postgres.Tests.Table;

public class TableSchemaGetCommandTests : CommandUnitTestsBase<TableSchemaGetCommand, IPostgresService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsSchema_WhenSchemaExists()
    {
        var expectedSchema = new List<string>(["CREATE TABLE test (id INT);"]);
        Service.GetTableSchemaAsync(AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", "table123", Arg.Any<CancellationToken>()).Returns(expectedSchema);

        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--table", "table123");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.TableSchemaGetCommandResult);
        Assert.Equal(expectedSchema, result.Schema);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenSchemaDoesNotExist()
    {
        Service.GetTableSchemaAsync(AuthTypes.MicrosoftEntra, "user1", null, "server1", "db123", "table123", Arg.Any<CancellationToken>()).Returns([]);

        var response = await ExecuteCommandAsync(
            $"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra,
            "--user", "user1",
            "--server", "server1",
            "--database", "db123",
            "--table", "table123");

        var result = ValidateAndDeserializeResponse(response, PostgresJsonContext.Default.TableSchemaGetCommandResult);
        Assert.Empty(result.Schema);
    }

    [Theory]
    [InlineData("--user")]
    [InlineData("--server")]
    [InlineData("--database")]
    [InlineData("--table")]
    public async Task ExecuteAsync_ReturnsError_WhenParameterIsMissing(string missingParameter)
    {
        var response = await ExecuteCommandAsync(ArgBuilder.BuildArgs(missingParameter,
            ($"--{PostgresOptionDefinitions.AuthTypeText}", AuthTypes.MicrosoftEntra),
            ("--user", "user1"),
            ("--server", "server123"),
            ("--database", "db123"),
            ("--table", "table123")
        ));

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains($"Missing Required options: {missingParameter}", response.Message);
    }

    [Fact]
    public void Command_DoesNotExposeArmScopingOptions()
    {
        var optionNames = CommandDefinition.Options.Select(o => o.Name.TrimStart('-')).ToList();

        Assert.DoesNotContain("subscription", optionNames);
        Assert.DoesNotContain("resource-group", optionNames);
        Assert.Contains("user", optionNames);
        Assert.Contains("server", optionNames);
        Assert.Contains("database", optionNames);
    }
}
