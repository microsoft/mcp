// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class TableSchemaCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKustoService _kusto;
    private readonly ILogger<TableSchemaCommand> _logger;

    public TableSchemaCommandTests()
    {
        _kusto = Substitute.For<IKustoService>();
        _logger = Substitute.For<ILogger<TableSchemaCommand>>();
        var collection = new ServiceCollection();
        collection.AddSingleton(_kusto);
        _serviceProvider = collection.BuildServiceProvider();
    }

    public static IEnumerable<object[]> TableSchemaArgumentMatrix()
    {
        yield return new object[] { "--subscription sub1 --cluster mycluster --database db1 --table table1", false };
        yield return new object[] { "--cluster-uri https://mycluster.kusto.windows.net --database db1 --table table1", true };
    }

    [Theory]
    [MemberData(nameof(TableSchemaArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsSchema(string cliArgs, bool useClusterUri)
    {
        var expectedSchema = "col1:datetime,col2:string";

        if (useClusterUri)
        {
            _kusto.GetTableSchema(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedSchema);
        }
        else
        {
            _kusto.GetTableSchema(
                "sub1", "mycluster", "db1", "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedSchema);
        }
        var command = new TableSchemaCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = System.Text.Json.JsonSerializer.Serialize(response.Results);
        var result = System.Text.Json.JsonSerializer.Deserialize(json, KustoJsonContext.Default.TableSchemaCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Schema);

        Assert.Equal(expectedSchema, result.Schema);
    }

    [Theory]
    [MemberData(nameof(TableSchemaArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsNull_WhenNoSchema(string cliArgs, bool useClusterUri)
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";

        if (useClusterUri)
        {
            _kusto.GetTableSchema(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .ThrowsAsync(new Exception("Test error"));
        }
        else
        {
            _kusto.GetTableSchema(
                "sub1", "mycluster", "db1", "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .ThrowsAsync(new Exception("Test error"));
        }
        var command = new TableSchemaCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Theory]
    [MemberData(nameof(TableSchemaArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            _kusto.GetTableSchema(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromException<string>(new Exception("Test error")));
        }
        else
        {
            _kusto.GetTableSchema(
                "sub1", "mycluster", "db1", "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromException<string>(new Exception("Test error")));
        }
        var command = new TableSchemaCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var command = new TableSchemaCommand(_logger);

        var args = command.GetCommand().Parse("");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
