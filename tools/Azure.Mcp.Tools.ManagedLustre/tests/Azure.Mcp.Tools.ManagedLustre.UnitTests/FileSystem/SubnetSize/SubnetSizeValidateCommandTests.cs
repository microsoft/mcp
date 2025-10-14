// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem;

public class FileSystemCheckSubnetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _amlfsService;
    private readonly ILogger<SubnetSizeValidateCommand> _logger;
    private readonly SubnetSizeValidateCommand _command;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;
    private readonly string _knownSubscriptionId = "sub123";

    public FileSystemCheckSubnetCommandTests()
    {
        _amlfsService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<SubnetSizeValidateCommand>>();

        var services = new ServiceCollection().AddSingleton(_amlfsService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("validate", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_ForValidInput()
    {
        // Arrange
        _amlfsService.CheckAmlFSSubnetAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions?>())
            .Returns(Task.FromResult(true));

        // Arrange
        var args = _commandDefinition.Parse([
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "48",
            "--location", "eastus",
            "--subnet-id", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<ResultJson>(json);
        Assert.NotNull(result);
        Assert.True(result!.Valid);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSku_Returns400()
    {
        // Arrange
        var args = _commandDefinition.Parse([
            "--sku", "INVALID-SKU",
            "--size", "48",
            "--location", "eastus",
            "--subnet-id", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.BadRequest);
        Assert.Contains("invalid sku", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_IsHandled()
    {
        // Arrange
        _amlfsService.CheckAmlFSSubnetAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions?>())
            .ThrowsAsync(new Exception("error"));

        var args = _commandDefinition.Parse([
            "--sku", "AMLFS-Durable-Premium-40",
            "--size", "48",
            "--location", "eastus",
            "--subnet-id", "/subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1",
            "--subscription", _knownSubscriptionId
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.True(response.Status >= HttpStatusCode.InternalServerError);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --location eastus --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1 --subscription sub123", true)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --location eastus --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1", false)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1 --subscription sub123", false)]
    [InlineData(" --size 48 --location eastus --subnet-id /subscriptions/sub123/resourceGroups/rg/providers/Microsoft.Network/virtualNetworks/vnet/subnets/sn1 --subscription sub123", false)]
    [InlineData("--sku AMLFS-Durable-Premium-40 --size 48 --location eastus --subscription sub123", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var parsedArgs = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parsedArgs);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
    }

    private class ResultJson
    {
        [JsonPropertyName("valid")]
        public bool Valid { get; set; }
    }
}
