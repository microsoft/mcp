// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.Storage.Commands;
using Azure.Mcp.Tools.Storage.Services;
using Azure.Mcp.Tools.Storage.Table.Commands;
using Microsoft.Mcp.Core.Options;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Storage.Tests.Table;

public class TableListCommandTests : CommandUnitTestsBase<TableListCommand, IStorageService>
{
    private readonly string _knownStorageAccount = "storage123";

    [Fact]
    public void Constructor_UsesStandardCursorDescription()
    {
        var cursor = Assert.Single(CommandDefinition.Options, option => option.Name == "--cursor");
        Assert.Equal(ICursorOption.CursorDescription, cursor.Description);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsStorageTables()
    {
        // Arrange
        var expectedTables = new List<string> { "table1", "table2" };

        Service.ListTables(
            Arg.Is(_knownStorageAccount),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((expectedTables, null));

        // Act
        var response = await ExecuteCommandAsync("--account", _knownStorageAccount);

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
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(([], null));

        // Act
        var response = await ExecuteCommandAsync("--account", _knownStorageAccount);

        // Assert
        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.TableListCommandResult);

        Assert.Empty(result.Tables);
    }

    [Theory]
    [InlineData("")] // No arguments
    public async Task ExecuteAsync_ValidatesMissingAccount(string args)
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
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync("--account", _knownStorageAccount);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
        Assert.DoesNotContain("--subscription", CommandDefinition.Options.Select(option => option.Name));
    }

    [Fact]
    public async Task ExecuteAsync_BindsCursorToTableListingArguments()
    {
        const string marker = " next+/=% token ";
        Service.ListTables(_knownStorageAccount, null, "tenant", Arg.Any<CancellationToken>()).Returns(([], marker));
        Service.ListTables(_knownStorageAccount, marker, "tenant", Arg.Any<CancellationToken>()).Returns(([], null));
        string[] args = ["--tenant", "tenant"];

        var first = await ExecuteCommandAsync([.. args, "--account", _knownStorageAccount]);
        var result = ValidateAndDeserializeResponse(first, StorageJsonContext.Default.TableListCommandResult);
        Assert.Empty(result.Tables);
        Assert.NotNull(result.NextCursor);
        Assert.NotEqual(marker, result.NextCursor);

        var final = await ExecuteCommandAsync([.. args, "--account", _knownStorageAccount, "--cursor", result.NextCursor]);
        var finalResult = ValidateAndDeserializeResponse(final, StorageJsonContext.Default.TableListCommandResult);
        var payload = JsonSerializer.SerializeToElement(finalResult, StorageJsonContext.Default.TableListCommandResult);
        Assert.False(payload.TryGetProperty("nextCursor", out _));
        await Service.Received(1).ListTables(_knownStorageAccount, marker, "tenant", Arg.Any<CancellationToken>());

        Service.ClearReceivedCalls();
        var mismatch = await ExecuteCommandAsync([.. args, "--account", "storage456", "--cursor", result.NextCursor]);
        Assert.Equal(HttpStatusCode.BadRequest, mismatch.Status);
        Assert.Contains("does not match the original request", mismatch.Message);
        Assert.Empty(Service.ReceivedCalls());
    }
}
