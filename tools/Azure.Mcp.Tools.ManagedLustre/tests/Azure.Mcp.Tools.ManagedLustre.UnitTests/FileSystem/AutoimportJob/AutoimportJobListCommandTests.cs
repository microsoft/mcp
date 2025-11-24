// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoimportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.AutoimportJob;

public class AutoimportJobListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _managedLustreService;
    private readonly ILogger<AutoimportJobListCommand> _logger;
    private readonly AutoimportJobListCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystemName = "fs1";

    public AutoimportJobListCommandTests()
    {
        _managedLustreService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<AutoimportJobListCommand>>();

        var services = new ServiceCollection().AddSingleton(_managedLustreService);
        _serviceProvider = services.BuildServiceProvider();

        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var cmd = _command.GetCommand();
        Assert.Equal("list", cmd.Name);
        Assert.False(string.IsNullOrWhiteSpace(cmd.Description));
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithRequiredParameters()
    {
        // Arrange
        var expectedJobs = new List<Models.AutoimportJob>
        {
            new()
            {
                Name = "job1",
                Id = $"/subscriptions/{_subscription}/resourceGroups/{_resourceGroup}/providers/Microsoft.StorageCache/amlFilesystems/{_fileSystemName}/autoImportJobs/job1",
                ProvisioningState = "Succeeded"
            },
            new()
            {
                Name = "job2",
                Id = $"/subscriptions/{_subscription}/resourceGroups/{_resourceGroup}/providers/Microsoft.StorageCache/amlFilesystems/{_fileSystemName}/autoImportJobs/job2",
                ProvisioningState = "Running"
            }
        };

        _managedLustreService.ListAutoimportJobsAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _managedLustreService.Received(1).ListAutoimportJobsAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithEmptyList()
    {
        // Arrange
        var expectedJobs = new List<Models.AutoimportJob>();

        _managedLustreService.ListAutoimportJobsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Theory]
    [InlineData("--resource-group rg1 --filesystem-name fs1", false)] // missing subscription
    [InlineData("--subscription sub123 --filesystem-name fs1", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // missing filesystem-name
    public async Task ExecuteAsync_ValidationErrors_Return400(string argLine, bool shouldSucceed)
    {
        // Arrange
        var args = _commandDefinition.Parse(argLine.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        var expectedStatus = shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
        Assert.Equal(expectedStatus, response.Status);
        if (!shouldSucceed)
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_RequestFailed_UsesStatusCode()
    {
        // Arrange
        _managedLustreService.ListAutoimportJobsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(404, "Filesystem not found"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", "nonexistent-fs"
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        Assert.Contains("not found", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_GenericException_Returns500()
    {
        // Arrange
        _managedLustreService.ListAutoimportJobsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var expectedJobs = new List<Models.AutoimportJob>
        {
            new()
            {
                Name = "job1",
                Id = "/subscriptions/sub/resourceGroups/rg/providers/Microsoft.StorageCache/amlFilesystems/fs/autoImportJobs/job1",
                ProvisioningState = "Succeeded"
            }
        };

        _managedLustreService.ListAutoimportJobsAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert - verify command executed successfully with expected parameters
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).ListAutoimportJobsAsync(
            _subscription,
            _resourceGroup,
            _fileSystemName,
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }
}
