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

public sealed class TableSchemaCommandTests : CommandUnitTestsBase<TableSchemaCommand, IKustoService>
{
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
            Service.GetTableSchemaAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedSchema);
        }
        else
        {
            Service.GetTableSchemaAsync(
                "sub1", "mycluster", "db1", "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedSchema);
        }

        var response = await ExecuteCommandAsync(cliArgs);

        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.TableSchemaCommandResult);

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
            Service.GetTableSchemaAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }
        else
        {
            Service.GetTableSchemaAsync(
                "sub1", "mycluster", "db1", "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }

        var response = await ExecuteCommandAsync(cliArgs);

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
            Service.GetTableSchemaAsync(
                "https://mycluster.kusto.windows.net",
                "db1",
                "table1",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }
        else
        {
            Service.GetTableSchemaAsync(
                "sub1", "mycluster", "db1", "table1",
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
