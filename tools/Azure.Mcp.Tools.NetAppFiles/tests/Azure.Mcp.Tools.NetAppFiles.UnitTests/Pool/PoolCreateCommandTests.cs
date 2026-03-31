// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.NetAppFiles.Commands;
using Azure.Mcp.Tools.NetAppFiles.Commands.Pool;
using Azure.Mcp.Tools.NetAppFiles.Models;
using Azure.Mcp.Tools.NetAppFiles.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.NetAppFiles.UnitTests.Pool;

public class PoolCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INetAppFilesService _netAppFilesService;
    private readonly ILogger<PoolCreateCommand> _logger;
    private readonly PoolCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    public PoolCreateCommandTests()
    {
        _netAppFilesService = Substitute.For<INetAppFilesService>();
        _logger = Substitute.For<ILogger<PoolCreateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_netAppFilesService);

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
    }

    [Theory]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123", true)]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123 --serviceLevel Premium", true)]
    [InlineData("--account myanfaccount --pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123 --qosType Auto", true)]
    [InlineData("--pool mypool --resource-group myrg --location eastus --size 4398046511104 --subscription sub123", false)] // Missing account
    [InlineData("--account myanfaccount --resource-group myrg --location eastus --size 4398046511104 --subscription sub123", false)] // Missing pool
    [InlineData("--account myanfaccount --pool mypool --location eastus --size 4398046511104 --subscription sub123", false)] // Missing resource-group
    [InlineData("", false)] // No parameters
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            var expectedPool = new CapacityPoolCreateResult(
                Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool",
                Name: "myanfaccount/mypool",
                Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
                Location: "eastus",
                ResourceGroup: "myrg",
                ProvisioningState: "Succeeded",
                ServiceLevel: "Premium",
                Size: 4398046511104,
                QosType: "Auto",
                CoolAccess: false,
                EncryptionType: "Single");

            _netAppFilesService.CreatePool(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<long>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<bool?>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<RetryPolicyOptions>(),
                Arg.Any<CancellationToken>())
                .Returns(expectedPool);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            Assert.Contains("required", response.Message.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteAsync_CreatesPool_Successfully()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        long size = 4398046511104;

        var expectedPool = new CapacityPoolCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}",
            Name: $"{account}/{pool}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Premium",
            Size: size,
            QosType: "Auto",
            CoolAccess: false,
            EncryptionType: "Single");

        _netAppFilesService.CreatePool(
            Arg.Is(account), Arg.Is(pool),
            Arg.Is(resourceGroup), Arg.Is(location), Arg.Is(size), Arg.Is(subscription),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPool));

        var args = _commandDefinition.Parse([
            "--account", account, "--pool", pool,
            "--resource-group", resourceGroup, "--location", location,
            "--size", size.ToString(), "--subscription", subscription
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Pool);
        Assert.Equal($"{account}/{pool}", result.Pool.Name);
        Assert.Equal(location, result.Pool.Location);
        Assert.Equal(resourceGroup, result.Pool.ResourceGroup);
        Assert.Equal("Succeeded", result.Pool.ProvisioningState);
        Assert.Equal("Premium", result.Pool.ServiceLevel);
        Assert.Equal(size, result.Pool.Size);
        Assert.Equal("Auto", result.Pool.QosType);
        Assert.Equal(false, result.Pool.CoolAccess);
        Assert.Equal("Single", result.Pool.EncryptionType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesException()
    {
        // Arrange
        var expectedError = "Test error";

        _netAppFilesService.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception(expectedError));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains(expectedError, response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflict()
    {
        // Arrange
        _netAppFilesService.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Conflict, "Pool already exists"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("already exists", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFound()
    {
        // Arrange
        _netAppFilesService.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.NotFound, "Resource group not found"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "nonexistentrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesAuthorizationFailure()
    {
        // Arrange
        _netAppFilesService.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new RequestFailedException((int)HttpStatusCode.Forbidden, "Authorization failed"));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Contains("Authorization failed", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesServiceErrors()
    {
        // Arrange
        _netAppFilesService.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CapacityPoolCreateResult>(new Exception("Test error")));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "4398046511104", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Test error", response.Message);
        Assert.Contains("troubleshooting", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedPool = new CapacityPoolCreateResult(
            Id: "/subscriptions/sub123/resourceGroups/myrg/providers/Microsoft.NetApp/netAppAccounts/myanfaccount/capacityPools/mypool",
            Name: "myanfaccount/mypool",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: "eastus",
            ResourceGroup: "myrg",
            ProvisioningState: "Succeeded",
            ServiceLevel: "Ultra",
            Size: 8796093022208,
            QosType: "Manual",
            CoolAccess: true,
            EncryptionType: "Double");

        _netAppFilesService.CreatePool(
            Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<long>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expectedPool));

        var args = _commandDefinition.Parse([
            "--account", "myanfaccount", "--pool", "mypool",
            "--resource-group", "myrg", "--location", "eastus",
            "--size", "8796093022208", "--subscription", "sub123"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, NetAppFilesJsonContext.Default.PoolCreateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Pool);
        Assert.Equal("myanfaccount/mypool", result.Pool.Name);
        Assert.Equal("eastus", result.Pool.Location);
        Assert.Equal("myrg", result.Pool.ResourceGroup);
        Assert.Equal("Succeeded", result.Pool.ProvisioningState);
        Assert.Equal("Ultra", result.Pool.ServiceLevel);
        Assert.Equal(8796093022208, result.Pool.Size);
        Assert.Equal("Manual", result.Pool.QosType);
        Assert.Equal(true, result.Pool.CoolAccess);
        Assert.Equal("Double", result.Pool.EncryptionType);
    }

    [Fact]
    public async Task ExecuteAsync_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var account = "myanfaccount";
        var pool = "mypool";
        var resourceGroup = "myrg";
        var location = "eastus";
        var subscription = "sub123";
        long size = 4398046511104;

        var expectedPool = new CapacityPoolCreateResult(
            Id: $"/subscriptions/{subscription}/resourceGroups/{resourceGroup}/providers/Microsoft.NetApp/netAppAccounts/{account}/capacityPools/{pool}",
            Name: $"{account}/{pool}",
            Type: "Microsoft.NetApp/netAppAccounts/capacityPools",
            Location: location,
            ResourceGroup: resourceGroup,
            ProvisioningState: "Succeeded",
            ServiceLevel: "Premium",
            Size: size,
            QosType: "Auto",
            CoolAccess: false,
            EncryptionType: "Single");

        _netAppFilesService.CreatePool(
            account, pool, resourceGroup, location, size, subscription,
            "Premium", Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(expectedPool);

        var args = _commandDefinition.Parse([
            "--account", account, "--pool", pool,
            "--resource-group", resourceGroup, "--location", location,
            "--size", size.ToString(), "--subscription", subscription,
            "--serviceLevel", "Premium"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _netAppFilesService.Received(1).CreatePool(
            account, pool, resourceGroup, location, size, subscription,
            "Premium", Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<string>(),
            null, Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>());
    }
}
