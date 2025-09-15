using System.CommandLine;
using System.Linq;
using System.Text.Json;
using Azure.Mcp.Core.Models;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Cosmos.Commands;
using Azure.Mcp.Tools.Cosmos.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using static Azure.Mcp.Tools.Cosmos.Commands.ItemQueryCommand;

namespace Azure.Mcp.Tools.Cosmos.UnitTests;

public class ItemQueryCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<ItemQueryCommand> _logger;
    private readonly ItemQueryCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ItemQueryCommandTests()
    {
        _cosmosService = Substitute.For<ICosmosService>();
        _logger = Substitute.For<ILogger<ItemQueryCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
        _serviceProvider = new ServiceCollection()
            .AddSingleton(_cosmosService)
            .BuildServiceProvider();
        _context = new(_serviceProvider);
    }

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

        _cosmosService.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is(query),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(), null, Arg.Any<RetryPolicyOptions>())
            .Returns(Task.FromResult(expectedItems));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ItemQueryCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
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

        _cosmosService.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is(query),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(expectedItems));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123"
            // No --query option
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ItemQueryCommandResult>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNull_WhenNoItemsExist()
    {
        // Arrange
        _cosmosService.QueryItems(
            Arg.Is<string>(s => s == "account123"),
            Arg.Is<string>(d => d == "database123"),
            Arg.Is<string>(c => c == "container123"),
            Arg.Is<string?>(q => q == null),
            Arg.Is<string>(s => s == "sub123"),
            Arg.Is<AuthMethod>(a => a == AuthMethod.Credential),
            Arg.Is<string?>(t => t == null),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(new List<JsonElement>());

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Null(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_Returns500_WhenServiceThrowsException()
    {
        // Arrange
        var expectedError = "Test error";
        var query = "SELECT * FROM c";

        _cosmosService.QueryItems(
            Arg.Is("account123"),
            Arg.Is("database123"),
            Arg.Is("container123"),
            Arg.Is(query),
            Arg.Is("sub123"),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(500, response.Status);
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
        var response = await _command.ExecuteAsync(_context, _commandDefinition.Parse(args));

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("required", response.Message.ToLower());
    }

    [Theory]
    [InlineData("DELETE FROM c")] // DELETE statement
    [InlineData("INSERT INTO c VALUES ('test')")] // INSERT statement
    [InlineData("UPDATE c SET name = 'test'")] // UPDATE statement
    [InlineData("DROP TABLE c")] // DROP statement
    [InlineData("CREATE TABLE test (id int)")] // CREATE statement
    [InlineData("ALTER TABLE c ADD column test")] // ALTER statement
    [InlineData("SELECT * FROM c WHERE 1=1 OR 1=1")] // Potential SQL injection pattern
    [InlineData("SELECT * FROM c UNION SELECT * FROM d")] // UNION injection attempt
    [InlineData("SELECT * FROM c; DROP TABLE c")] // Multiple statements
    [InlineData("SELECT * FROM c --comment")] // Comment injection
    [InlineData("SELECT * FROM c /* comment */")] // Block comment
    public async Task ExecuteAsync_Returns400_WhenQueryContainsDangerousPatterns(string maliciousQuery)
    {
        // Arrange
        _cosmosService.QueryItems(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            maliciousQuery,
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException($"Query contains dangerous patterns which are not allowed for security reasons."));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", maliciousQuery
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("security", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenQueryIsTooLong()
    {
        // Arrange
        var longQuery = "SELECT * FROM c WHERE " + string.Join(" OR ", Enumerable.Range(1, 1000).Select(i => $"c.field{i} = 'value{i}'"));
        
        _cosmosService.QueryItems(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            longQuery,
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException("Query exceeds maximum length to prevent DoS attacks."));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", longQuery
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("DoS", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenQueryIsEmpty()
    {
        // Arrange
        _cosmosService.QueryItems(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            "",
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException("Query cannot be empty."));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", ""
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("empty", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_Returns400_WhenQueryContainsTooManyWildcards()
    {
        // Arrange
        var query = "SELECT * FROM * WHERE * = * AND * > * OR * < *"; // 7 wildcards, exceeds limit of 5
        
        _cosmosService.QueryItems(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            query,
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>())
            .ThrowsAsync(new InvalidOperationException("Query contains too many wildcard operators (*) which could cause performance issues."));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", query
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.Equal(400, response.Status);
        Assert.Contains("wildcard", response.Message.ToLower());
    }

    [Theory]
    [InlineData("SELECT c.id FROM c")] // Simple valid query
    [InlineData("SELECT * FROM c WHERE c.type = 'product'")] // Valid with WHERE clause
    [InlineData("SELECT TOP 10 c.id, c.name FROM c")] // Valid with TOP
    [InlineData("SELECT c.id FROM c ORDER BY c.timestamp DESC")] // Valid with ORDER BY
    [InlineData("SELECT c.id FROM c WHERE CONTAINS(c.description, 'test')")] // Valid with CONTAINS function
    public async Task ExecuteAsync_SuccessfullyExecutes_WhenQueryIsValid(string validQuery)
    {
        // Arrange
        var expectedItems = new List<JsonElement>
        {
            JsonDocument.Parse("{\"id\":\"item1\"}").RootElement.Clone()!
        };

        _cosmosService.QueryItems(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Is(validQuery),
            Arg.Any<string>(),
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(expectedItems));

        var args = _commandDefinition.Parse([
            "--account", "account123",
            "--database", "database123",
            "--container", "container123",
            "--subscription", "sub123",
            "--query", validQuery
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(500, response.Status); // Should not be a server error
        
        // Verify that the CosmosService was called with the validated query
        await _cosmosService.Received(1).QueryItems(
            "account123",
            "database123",
            "container123",
            validQuery,
            "sub123",
            Arg.Any<AuthMethod>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>());
    }
}
