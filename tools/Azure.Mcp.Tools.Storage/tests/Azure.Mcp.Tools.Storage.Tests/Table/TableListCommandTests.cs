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

    [Theory]
    [InlineData(null, " next+/=% token ")]
    [InlineData(" next+/=% token ", " last+/=% token ")]
    [InlineData(" last+/=% token ", null)]
    [InlineData(null, null)]
    public async Task ExecuteAsync_PreservesOpaqueCursorsOnEmptyPages(string? cursor, string? nextCursor)
    {
        Service.ListTables(_knownStorageAccount, cursor, "tenant", Arg.Any<CancellationToken>())
            .Returns(([], nextCursor));
        var args = new List<string> { "--account", _knownStorageAccount, "--tenant", "tenant" };
        if (cursor is not null)
        {
            args.AddRange(["--cursor", cursor]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var result = ValidateAndDeserializeResponse(response, StorageJsonContext.Default.TableListCommandResult);
        Assert.Empty(result.Tables);
        Assert.Equal(nextCursor, result.NextCursor);
        var payload = JsonSerializer.SerializeToElement(result, StorageJsonContext.Default.TableListCommandResult);
        Assert.Equal(nextCursor is not null, payload.TryGetProperty("nextCursor", out _));
        await Service.Received(1).ListTables(_knownStorageAccount, cursor, "tenant", Arg.Any<CancellationToken>());
    }
}
