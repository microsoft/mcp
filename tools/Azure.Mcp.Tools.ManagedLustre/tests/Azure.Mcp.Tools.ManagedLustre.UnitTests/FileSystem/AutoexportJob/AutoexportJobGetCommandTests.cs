// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.AutoexportJob;

public class AutoexportJobGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _managedLustreService;
    private readonly ILogger<AutoexportJobGetCommand> _logger;
    private readonly AutoexportJobGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;
    private readonly string _subscription = "sub123";
    private readonly string _resourceGroup = "rg1";
    private readonly string _fileSystemName = "fs1";
    private readonly string _jobName = "job1";

    public AutoexportJobGetCommandTests()
    {
        _managedLustreService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<AutoexportJobGetCommand>>();

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
        Assert.Equal("get", cmd.Name);
        Assert.False(string.IsNullOrWhiteSpace(cmd.Description));
    }

    [Fact]
    public async Task ExecuteAsync_Succeeds_WithRequiredParameters()
    {
        // Arrange
        var expectedJob = new Models.AutoexportJob
        {
            Name = _jobName,
            Id = $"/subscriptions/{_subscription}/resourceGroups/{_resourceGroup}/providers/Microsoft.StorageCache/amlFilesystems/{_fileSystemName}/autoExportJobs/{_jobName}",
            ProvisioningState = "Succeeded"
        };

        _managedLustreService.GetAutoexportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJob);

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--job-name", _jobName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _managedLustreService.Received(1).GetAutoexportJobAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Is(_jobName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("--resource-group rg1 --filesystem-name fs1 --job-name job1", false)] // missing subscription
    [InlineData("--subscription sub123 --filesystem-name fs1 --job-name job1", false)] // missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1 --job-name job1", false)] // missing filesystem-name
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name fs1", true)] // valid without job-name (list all)
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
        _managedLustreService.GetAutoexportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .ThrowsAsync(new Azure.RequestFailedException(404, "Autoexport job not found"));

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName,
            "--job-name", "nonexistent-job"
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
        _managedLustreService.GetAutoexportJobAsync(
            Arg.Any<string>(),
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
            "--filesystem-name", _fileSystemName,
            "--job-name", _jobName
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("error", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ExecuteAsync_ListsAllJobs_WhenJobNameNotProvided()
    {
        // Arrange
        var expectedJobs = new List<Models.AutoexportJob>
        {
            new() { Name = "job1", Id = $"/subscriptions/{_subscription}/resourceGroups/{_resourceGroup}/providers/Microsoft.StorageCache/amlFilesystems/{_fileSystemName}/autoExportJobs/job1", ProvisioningState = "Succeeded" },
            new() { Name = "job2", Id = $"/subscriptions/{_subscription}/resourceGroups/{_resourceGroup}/providers/Microsoft.StorageCache/amlFilesystems/{_fileSystemName}/autoExportJobs/job2", ProvisioningState = "Running" }
        };

        _managedLustreService.ListAutoexportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedJobs);

        var args = _commandDefinition.Parse([
            "--subscription", _subscription,
            "--resource-group", _resourceGroup,
            "--filesystem-name", _fileSystemName
            // Intentionally omitting --job-name
        ]);

        // Act
        var response = await _command.ExecuteAsync(_context, args, CancellationToken.None);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        await _managedLustreService.Received(1).ListAutoexportJobsAsync(
            Arg.Is(_subscription),
            Arg.Is(_resourceGroup),
            Arg.Is(_fileSystemName),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>());

        // Should NOT have called GetAutoexportJobAsync
        await _managedLustreService.DidNotReceive().GetAutoexportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }
}
