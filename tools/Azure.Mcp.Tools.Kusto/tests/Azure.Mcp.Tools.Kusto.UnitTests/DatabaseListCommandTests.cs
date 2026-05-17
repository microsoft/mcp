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

public sealed class DatabaseListCommandTests : CommandUnitTestsBase<DatabaseListCommand, IKustoService>
{
    public static IEnumerable<object[]> DatabaseArgumentMatrix()
    {
        yield return new object[] { "--subscription sub1 --cluster mycluster", false };
        yield return new object[] { "--cluster-uri https://mycluster.kusto.windows.net", true };
    }

    [Theory]
    [MemberData(nameof(DatabaseArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsDatabases(string cliArgs, bool useClusterUri)
    {
        // Arrange
        var expectedDatabases = new List<string> { "db1", "db2" };
        if (useClusterUri)
        {
            Service.ListDatabasesAsync(
                "https://mycluster.kusto.windows.net",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedDatabases);
        }
        else
        {
            Service.ListDatabasesAsync(
                "sub1", "mycluster", Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns(expectedDatabases);
        }

        // Act
        var response = await ExecuteCommandAsync(cliArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.DatabaseListCommandResult);
        Assert.Equal(expectedDatabases, result.Databases);
    }

    [Theory]
    [MemberData(nameof(DatabaseArgumentMatrix))]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoDatabasesExist(string cliArgs, bool useClusterUri)
    {
        // Arrange
        if (useClusterUri)
        {
            Service.ListDatabasesAsync(
                "https://mycluster.kusto.windows.net",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }
        else
        {
            Service.ListDatabasesAsync(
                "sub1", "mycluster", Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .Returns([]);
        }

        // Act
        var response = await ExecuteCommandAsync(cliArgs);

        // Assert
        var result = ValidateAndDeserializeResponse(response, KustoJsonContext.Default.DatabaseListCommandResult);
        Assert.Empty(result.Databases!);
    }

    [Theory]
    [MemberData(nameof(DatabaseArgumentMatrix))]
    public async Task ExecuteAsync_HandlesException_AndSetsException(string cliArgs, bool useClusterUri)
    {
        // Arrange
        var expectedError = "Test error. To mitigate this issue, please refer to the troubleshooting guidelines here at https://aka.ms/azmcp/troubleshooting.";
        if (useClusterUri)
        {
            Service.ListDatabasesAsync(
                "https://mycluster.kusto.windows.net",
                Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }
        else
        {
            Service.ListDatabasesAsync(
                "sub1", "mycluster", Arg.Any<string>(), Arg.Any<AuthMethod?>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("Test error"));
        }

        // Act
        var response = await ExecuteCommandAsync(cliArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Equal(expectedError, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsBadRequest_WhenMissingAllRequiredOptions()
    {
        var response = await ExecuteCommandAsync("");

        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Either --cluster-uri must be provided", response.Message, StringComparison.OrdinalIgnoreCase);
    }
}
