// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Azure;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<ServerListCommand> _logger;
    private readonly ServerListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServerListCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ServerListCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_service);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("list", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Lists Azure SQL servers", command.Description);
    }

    [Fact]
    public void CommandMetadata_IsConfiguredCorrectly()
    {
        var metadata = _command.Metadata;
        Assert.False(metadata.Destructive);
        Assert.True(metadata.Idempotent);
        Assert.True(metadata.OpenWorld);
        Assert.True(metadata.ReadOnly);
        Assert.False(metadata.LocalRequired);
        Assert.False(metadata.Secret);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg", true)]
    [InlineData("--subscription sub", false)] // Missing resource group
    [InlineData("--resource-group rg", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedServers = new List<SqlServer>
            {
                new(
                    Name: "testserver1",
                    FullyQualifiedDomainName: "testserver1.database.windows.net",
                    Location: "East US",
                    ResourceGroup: "rg",
                    Subscription: "sub",
                    AdministratorLogin: "admin",
                    Version: "12.0",
                    State: "Ready",
                    PublicNetworkAccess: "Enabled",
                    Tags: new Dictionary<string, string>())
            };

            _service.ListServersAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<Core.Options.RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedServers);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(shouldSucceed ? 200 : 400, response.Status);
        if (shouldSucceed)
        {
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_ListsServersSuccessfully()
    {
        // Arrange
        var expectedServers = new List<SqlServer>
        {
            new(
                Name: "testserver1",
                FullyQualifiedDomainName: "testserver1.database.windows.net",
                Location: "East US",
                ResourceGroup: "rg",
                Subscription: "sub",
                AdministratorLogin: "admin1",
                Version: "12.0",
                State: "Ready",
                PublicNetworkAccess: "Enabled",
                Tags: new Dictionary<string, string>()),
            new(
                Name: "testserver2",
                FullyQualifiedDomainName: "testserver2.database.windows.net",
                Location: "West US",
                ResourceGroup: "rg",
                Subscription: "sub",
                AdministratorLogin: "admin2",
                Version: "12.0",
                State: "Ready",
                PublicNetworkAccess: "Disabled",
                Tags: new Dictionary<string, string> { { "Environment", "Test" } })
        };

        _service.ListServersAsync(
            "rg",
            "sub",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServers);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _service.Received(1).ListServersAsync(
            "rg",
            "sub",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyList_ReturnsSuccessfully()
    {
        // Arrange
        var emptyServerList = new List<SqlServer>();

        _service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(emptyServerList);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _service.Received(1).ListServersAsync(
            "rg",
            "sub",
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        _service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServer>>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.NotEqual(200, response.Status);
        Assert.Contains("error", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenAuthorizationFails_Returns403StatusCode()
    {
        // Arrange
        var requestException = new RequestFailedException(403, "Forbidden: Insufficient permissions");

        _service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServer>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(403, response.Status);
        Assert.Contains("authorization failed", response.Message.ToLower());
        Assert.Contains("verify you have appropriate permissions", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenResourceGroupNotFound_Returns404StatusCode()
    {
        // Arrange
        var requestException = new RequestFailedException(404, "Not Found: Resource group does not exist");

        _service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServer>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group nonexistent-rg");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(404, response.Status);
        Assert.Contains("no sql servers found", response.Message.ToLower());
        Assert.Contains("verify the resource group and subscription", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WithGenericRequestFailedException_ReturnsOriginalMessage()
    {
        // Arrange
        var requestException = new RequestFailedException(500, "Internal Server Error");

        _service.ListServersAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<Core.Options.RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<SqlServer>>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.StartsWith("Internal Server Error", response.Message);
    }
}
