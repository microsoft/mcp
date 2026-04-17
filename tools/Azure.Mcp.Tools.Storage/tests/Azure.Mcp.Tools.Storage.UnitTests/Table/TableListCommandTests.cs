// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Mcp.Tools.Storage.Table.Commands;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.UnitTests.Table;

public class TableListCommandTests : CommandUnitTestsBase<TableListCommand, IStorageService>
{
    private readonly string _knownStorageAccount = "storage123";
    private readonly string _knownSubscription = "sub123";

    [Fact]
    public async Task ExecuteAsync_ReturnsStorageTables()
    {
        // Arrange
        var expectedTables = new List<string> { "table1", "table2" };

        Service.ListTables(
            Arg.Is(_knownStorageAccount),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedTables);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", _knownStorageAccount,
            "--subscription", _knownSubscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.TableListCommandResult);

        Assert.Equal(expectedTables, result.Tables);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoStorageTables()
    {
        // Arrange
        Service.ListTables(
            Arg.Is(_knownStorageAccount),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .Returns([]);

        // Act
        var response = await ExecuteCommandAsync(
            "--account", _knownStorageAccount,
            "--subscription", _knownSubscription);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.TableListCommandResult);

        Assert.Empty(result.Tables);
    }

    [Theory]
    [InlineData("--subscription sub123")] // Missing Storage account
    [InlineData("--account mystorageaccount")] // Missing subscription
    [InlineData("")] // No arguments
    public async Task ExecuteAsync_ValidatesMissingSubscriptionCorrectly(string args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesStorageException()
    {
        // Arrange
        var expectedError = "Test error";

        Service.ListTables(
            Arg.Is(_knownStorageAccount),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", _knownStorageAccount,
            "--subscription", _knownSubscription);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }
}
