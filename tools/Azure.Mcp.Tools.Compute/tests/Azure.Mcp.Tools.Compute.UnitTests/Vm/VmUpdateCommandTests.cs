// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Models;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.Compute.UnitTests.Vm;

public class VmUpdateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IComputeService _computeService;
    private readonly ILogger<VmUpdateCommand> _logger;
    private readonly VmUpdateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _knownSubscription = "sub123";
    private readonly string _knownResourceGroup = "test-rg";
    private readonly string _knownVmName = "test-vm";

    public VmUpdateCommandTests()
    {
        _computeService = Substitute.For<IComputeService>();
        _logger = Substitute.For<ILogger<VmUpdateCommand>>();

        var collection = new ServiceCollection().AddSingleton(_computeService);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("update", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --tags env=test", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --boot-diagnostics true", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --license-type Windows_Server", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123 --vm-size Standard_D4s_v3", true)]
    [InlineData("--vm-name test-vm --resource-group test-rg --subscription sub123", false)] // No update property
    [InlineData("--resource-group test-rg --subscription sub123 --tags env=test", false)] // Missing vm-name
    [InlineData("--vm-name test-vm --subscription sub123 --tags env=test", false)] // Missing resource-group
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var updateResult = new VmUpdateResult(
                Name: _knownVmName,
                Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
                Location: "eastus",
                VmSize: "Standard_D2s_v3",
                ProvisioningState: "Succeeded",
                PowerState: "VM running",
                OsType: "linux",
                LicenseType: null,
                Zones: null,
                Tags: new Dictionary<string, string> { { "env", "test" } });

            _computeService.UpdateVmAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(updateResult);
        }

        var parseResult = _commandDefinition.Parse(args);

        // Act & Assert
        if (shouldSucceed)
        {
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            Assert.Equal("Success", response.Message);
        }
        else
        {
            // For missing required options or validation failures
            try
            {
                var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);
                Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            }
            catch (Microsoft.Mcp.Core.Commands.CommandValidationException)
            {
                // Expected for validation failures
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVmWithTags()
    {
        // Arrange
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: null,
            Tags: new Dictionary<string, string> { { "env", "prod" }, { "team", "compute" } });

        _computeService.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Is("env=prod,team=compute"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=prod,team=compute"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
        Assert.NotNull(result.Vm.Tags);
        Assert.Equal(2, result.Vm.Tags.Count);
    }

    [Fact]
    public async Task ExecuteAsync_UpdatesVmWithLicenseType()
    {
        // Arrange
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "windows",
            LicenseType: "Windows_Server",
            Zones: null,
            Tags: null);

        _computeService.UpdateVmAsync(
            Arg.Is(_knownVmName),
            Arg.Is(_knownResourceGroup),
            Arg.Is(_knownSubscription),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Is("Windows_Server"),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--license-type", "Windows_Server"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmUpdateCommandResult);

        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Equal("Windows_Server", result.Vm.LicenseType);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesNotFoundError()
    {
        // Arrange
        var notFoundException = new RequestFailedException((int)HttpStatusCode.NotFound, "VM not found");

        _computeService.UpdateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(notFoundException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=test"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesConflictError()
    {
        // Arrange
        var conflictException = new RequestFailedException((int)HttpStatusCode.Conflict, "VM must be deallocated to change size");

        _computeService.UpdateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(conflictException);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--vm-size", "Standard_D4s_v3"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.Status);
        Assert.Contains("deallocated", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var expectedResult = new VmUpdateResult(
            Name: _knownVmName,
            Id: "/subscriptions/sub123/resourceGroups/test-rg/providers/Microsoft.Compute/virtualMachines/test-vm",
            Location: "eastus",
            VmSize: "Standard_D2s_v3",
            ProvisioningState: "Succeeded",
            PowerState: "VM running",
            OsType: "linux",
            LicenseType: null,
            Zones: ["1"],
            Tags: new Dictionary<string, string> { { "env", "test" } });

        _computeService.UpdateVmAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        var parseResult = _commandDefinition.Parse([
            "--vm-name", _knownVmName,
            "--resource-group", _knownResourceGroup,
            "--subscription", _knownSubscription,
            "--tags", "env=test"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response.Results);
        var json = JsonSerializer.Serialize(response.Results);

        var result = JsonSerializer.Deserialize(json, ComputeJsonContext.Default.VmUpdateCommandResult);
        Assert.NotNull(result);
        Assert.NotNull(result.Vm);
        Assert.Equal(_knownVmName, result.Vm.Name);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(
            $"--vm-name {_knownVmName} --resource-group {_knownResourceGroup} --subscription {_knownSubscription} --vm-size Standard_D4s_v3 --tags env=test --license-type Windows_Server --boot-diagnostics true --user-data dGVzdA==");

        // Assert parse was successful
        Assert.Empty(parseResult.Errors);
    }
}
