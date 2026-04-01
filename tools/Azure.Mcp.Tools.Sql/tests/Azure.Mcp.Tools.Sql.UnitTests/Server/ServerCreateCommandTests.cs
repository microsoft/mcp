// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.Sql.Commands.Server;
using Azure.Mcp.Tools.Sql.Models;
using Azure.Mcp.Tools.Sql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Options;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Sql.UnitTests.Server;

public class ServerCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ISqlService _service;
    private readonly ILogger<ServerCreateCommand> _logger;
    private readonly ServerCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public ServerCreateCommandTests()
    {
        _service = Substitute.For<ISqlService>();
        _logger = Substitute.For<ILogger<ServerCreateCommand>>();

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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
        Assert.Contains("Creates a new Azure SQL server", command.Description);
    }

    [Theory]
    [InlineData("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!", true)]
    [InlineData("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin", false)] // Missing password
    [InlineData("--subscription sub --resource-group rg --server testserver --location eastus --administrator-password Password123!", false)] // Missing login
    [InlineData("--subscription sub --resource-group rg --server testserver --administrator-login admin --administrator-password Password123!", false)] // Missing location
    [InlineData("--subscription sub --resource-group rg --location eastus --administrator-login admin --administrator-password Password123!", false)] // Missing server
    [InlineData("--subscription sub --server testserver --location eastus --administrator-login admin --administrator-password Password123!", false)] // Missing resource group
    [InlineData("--resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!", false)] // Missing subscription
    [InlineData("", false)] // Missing all required parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var expectedServer = new SqlServer(
                Name: "testserver",
                FullyQualifiedDomainName: "testserver.database.windows.net",
                Location: "East US",
                ResourceGroup: "rg",
                Subscription: "sub",
                AdministratorLogin: "admin",
                Version: "12.0",
                State: "Ready",
                PublicNetworkAccess: "Enabled",
                Tags: []
            );

            _service.CreateServerAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedServer);
        }

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
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
    public async Task ExecuteAsync_CreatesServerSuccessfully()
    {
        // Arrange
        var expectedServer = new SqlServer(
            Name: "testserver",
            FullyQualifiedDomainName: "testserver.database.windows.net",
            Location: "East US",
            ResourceGroup: "rg",
            Subscription: "sub",
            AdministratorLogin: "admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Enabled",
            Tags: []
        );

        _service.CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _service.Received(1).CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_PassesAllParameters()
    {
        // Arrange
        var expectedServer = new SqlServer(
            Name: "testserver",
            FullyQualifiedDomainName: "testserver.database.windows.net",
            Location: "East US",
            ResourceGroup: "rg",
            Subscription: "sub",
            AdministratorLogin: "admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Disabled",
            Tags: []
        );

        _service.CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            "12.0",
            "Disabled",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123! --version 12.0 --public-network-access Disabled");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);

        await _service.Received(1).CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            "12.0",
            "Disabled",
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutPublicNetworkAccess_PassesNullToService()
    {
        // Arrange
        // The service layer defaults publicNetworkAccess to Disabled when null is passed
        var expectedServer = new SqlServer(
            Name: "testserver",
            FullyQualifiedDomainName: "testserver.database.windows.net",
            Location: "East US",
            ResourceGroup: "rg",
            Subscription: "sub",
            AdministratorLogin: "admin",
            Version: "12.0",
            State: "Ready",
            PublicNetworkAccess: "Disabled",
            Tags: []
        );

        _service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedServer);

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        await _service.Received(1).CreateServerAsync(
            "testserver",
            "rg",
            "sub",
            "eastus",
            "admin",
            "Password123!",
            Arg.Is<string?>(v => v == null), // version not specified
            Arg.Is<string?>(p => p == null), // publicNetworkAccess not specified; service defaults to Disabled
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidPublicNetworkAccess_ReturnsBadRequest()
    {
        // Arrange
        _service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(new ArgumentException("Invalid value 'Enabeld' for public-network-access. Allowed values are 'Enabled' or 'Disabled'.", "publicNetworkAccess")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123! --public-network-access Enabeld");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Contains("Invalid parameter", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        _service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(new Exception("Test error")));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotEqual(HttpStatusCode.OK, response.Status);
        Assert.Contains("error", response.Message.ToLower());
    }

    [Fact]
    public async Task ExecuteAsync_WhenServerAlreadyExists_Returns409StatusCode()
    {
        // Arrange
        var requestException = new RequestFailedException((int)HttpStatusCode.Conflict, "Conflict: Server already exists");

        _service.CreateServerAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SqlServer>(requestException));

        var context = new CommandContext(_serviceProvider);
        var parseResult = _commandDefinition.Parse("--subscription sub --resource-group rg --server testserver --location eastus --administrator-login admin --administrator-password Password123!");

        // Act
        var response = await _command.ExecuteAsync(context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("server with this name already exists", response.Message.ToLower());
    }
}
