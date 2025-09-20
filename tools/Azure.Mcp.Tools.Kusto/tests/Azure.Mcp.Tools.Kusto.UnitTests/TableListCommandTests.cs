// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class TableListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IKustoService _kusto;
    private readonly ILogger<TableListCommand> _logger;

    public TableListCommandTests()
    {
        _kusto = Substitute.For<IKustoService>();
        _logger = Substitute.For<ILogger<TableListCommand>>();
        var collection = new ServiceCollection();
        collection.AddSingleton(_kusto);
        _serviceProvider = collection.BuildServiceProvider();
    }

    public static IEnumerable<object[]> TableListArgumentMatrix()
    {
        yield return new object[] { "--subscription sub1 --cluster mycluster --database db1", false };
        yield return new object[] { "--cluster-uri https://mycluster.kusto.windows.net --database db1", true };
    }

    [Theory]
    [MemberData(nameof(TableListArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsTables(string cliArgs, bool useClusterUri)
    {
        var expectedTables = new List<string> { "table1", "table2" };
        if (useClusterUri)
        {
            _kusto.ListTables(
                "https://mycluster.kusto.windows.net",
                "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedTables);
        }
        else
        {
            _kusto.ListTables(
                "sub1", "mycluster", "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(expectedTables);
        }
        var command = new TableListCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);

        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.TableListCommandResult);

        Assert.NotNull(result);
        Assert.Equal(2, result?.Tables?.Count);
    }

    [Theory]
    [MemberData(nameof(TableListArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoTables(string cliArgs, bool useClusterUri)
    {
        if (useClusterUri)
        {
            _kusto.ListTables(
                "https://mycluster.kusto.windows.net",
                "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns([]);
        }
        else
        {
            _kusto.ListTables(
                "sub1", "mycluster", "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns([]);
        }
        var command = new TableListCommand(_logger);

        var args = command.GetCommand().Parse(cliArgs);
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, KustoJsonContext.Default.TableListCommandResult);

        Assert.NotNull(result);
        Assert.Empty(result.Tables!);
    }

    [Theory]
    [MemberData(nameof(TableListArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            _kusto.ListTables(
                "https://mycluster.kusto.windows.net",
                "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromException<List<string>>(new Exception("Test error")));
        }
        else
        {
            _kusto.ListTables(
                "sub1", "mycluster", "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>())
                .Returns(Task.FromException<List<string>>(new Exception("Test error")));
        }
        var command = new TableListCommand(_logger);

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
        var command = new TableListCommand(_logger);

        var args = command.GetCommand().Parse("");
        var context = new CommandContext(_serviceProvider);

        var response = await command.ExecuteAsync(context, args);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
