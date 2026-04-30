// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Kusto.Commands;
using Azure.Mcp.Tools.Kusto.Services;
using Microsoft.Mcp.Core.Models;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Kusto.UnitTests;

public sealed class TableListCommandTests : CommandUnitTestsBase<TableListCommand, IKustoService>
{
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
            Service.ListTablesAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedTables);
        }
        else
        {
            Service.ListTablesAsync(
                "sub1", "mycluster", "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedTables);
        }

        var response = await ExecuteCommandAsync(cliArgs);

        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.TableListCommandResult);

        Assert.Equal(2, result.Tables?.Count);
    }

    [Theory]
    [MemberData(nameof(TableListArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoTables(string cliArgs, bool useClusterUri)
    {
        if (useClusterUri)
        {
            Service.ListTablesAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }
        else
        {
            Service.ListTablesAsync(
                "sub1", "mycluster", "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        var response = await ExecuteCommandAsync(cliArgs);

        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.TableListCommandResult);
        Assert.Empty(result.Tables);
    }

    [Theory]
    [MemberData(nameof(TableListArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            Service.ListTablesAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }
        else
        {
            Service.ListTablesAsync(
                "sub1", "mycluster", "db1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }

        var response = await ExecuteCommandAsync(cliArgs);
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingRequiredOptions()
    {
        var response = await ExecuteCommandAsync("");
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }
}
