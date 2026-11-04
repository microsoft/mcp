// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json;
using Azure.Mcp.Tests.Commands;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Mcp.Core.Models;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Cosmos.Tests.Item;

public class ItemQueryCommandTests : SubscriptionCommandUnitTestsBase<ItemQueryCommand, ICosmosService>
{
    [Fact]
    public async Task ExecuteAsync_ReturnsItems_WhenQueryIsProvided()
    {
        // Arrange
        var query = "SELECT * FROM c WHERE c.type = 'test'";
        var expectedItems = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"item1\"}").RootElement.Clone()!,
            JsonDocument.Parse("{\"id\":\"item2\"}").RootElement.Clone()!
        };

        Service.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is(query),
            Arg.Is("sub123"),
            null,
            Arg.Any<AuthMethod>(),
            null,
            Arg.Any<CancellationToken>())
            .Returns((expectedItems, null));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", query);

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemQueryCommandResult);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsItems_WhenNoQueryProvided()
    {
        // Arrange
        var query = "SELECT * FROM c";
        var expectedItems = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"item1\"}").RootElement.Clone()!,
            JsonDocument.Parse("{\"id\":\"item2\"}").RootElement.Clone()!
        };

        Service.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is(query),
            Arg.Is("sub123"),
            null,
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((expectedItems, null));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemQueryCommandResult);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsEmpty_WhenNoItemsExist()
    {
        // Arrange
        Service.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is("SELECT * FROM c"),
            Arg.Is("sub123"),
            null,
            Arg.Is(AuthMethod.Credential),
            Arg.Is((string?)null),
            Arg.Any<CancellationToken>())
            .Returns(([], null));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123");

        // Assert
        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemQueryCommandResult);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        var expectedError = "Test error";
        var query = "SELECT * FROM c";

        Service.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is(query),
            Arg.Is("sub123"),
            null,
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        // Act
        var response = await ExecuteCommandAsync(
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.StartsWith(expectedError, response.Message);
    }

    [Theory]
    [InlineData("--account", "account123", "--database", "database123", "--container", "container123")] // Missing subscription
    [InlineData("--subscription", "sub123", "--database", "database123", "--container", "container123")] // Missing account-name
    [InlineData("--subscription", "sub123", "--account", "account123", "--container", "container123")] // Missing database-name
    [InlineData("--subscription", "sub123", "--account", "account123", "--database", "database123")] // Missing container-name
    public async Task ExecuteAsync_Returns400_WhenRequiredParametersAreMissing(params string[] args)
    {
        // Arrange & Act
        var response = await ExecuteCommandAsync(args);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Theory]
    [InlineData(null, " next+/=% token ")]
    [InlineData(" next+/=% token ", " last+/=% token ")]
    [InlineData(" last+/=% token ", null)]
    [InlineData(null, null)]
    public async Task ExecuteAsync_PreservesOpaqueCursors(string? cursor, string? nextCursor)
    {
        var query = "SELECT * FROM c WHERE c.type = 'test'";
        using var document = JsonDocument.Parse("""{"Documents":[]}""");
        Service.QueryItems("account", "database", "container", query, "sub", cursor,
                AuthMethod.Credential, "tenant", Arg.Any<CancellationToken>())
            .Returns(([document.RootElement.Clone()], nextCursor));
        var args = new List<string>
        {
            "--account", "account", "--database", "database", "--container", "container",
            "--subscription", "sub", "--tenant", "tenant", "--query", query
        };
        if (cursor is not null)
        {
            args.AddRange(["--cursor", cursor]);
        }

        var response = await ExecuteCommandAsync(args.ToArray());

        var result = ValidateAndDeserializeResponse(response, CosmosJsonContext.Default.ItemQueryCommandResult);
        Assert.Empty(Assert.Single(result.Items).GetProperty("Documents").EnumerateArray());
        Assert.Equal(nextCursor, result.NextCursor);
        var payload = JsonSerializer.SerializeToElement(result, CosmosJsonContext.Default.ItemQueryCommandResult);
        Assert.Equal(nextCursor is not null, payload.TryGetProperty("nextCursor", out _));
        await Service.Received(1).QueryItems("account", "database", "container", query, "sub", cursor,
            AuthMethod.Credential, "tenant", Arg.Any<CancellationToken>());
    }
}
