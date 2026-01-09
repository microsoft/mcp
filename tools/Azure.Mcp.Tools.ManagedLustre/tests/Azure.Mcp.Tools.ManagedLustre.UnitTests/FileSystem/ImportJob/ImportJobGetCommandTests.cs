// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Models;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.ImportJob;

public class ImportJobGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _managedLustreService;
    private readonly ILogger<ImportJobGetCommand> _logger;
    private readonly ImportJobGetCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string JobName = "import-job-01";

    public ImportJobGetCommandTests()
    {
        _managedLustreService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<ImportJobGetCommand>>();
        var services = new ServiceCollection().AddSingleton(_managedLustreService);
        _serviceProvider = services.BuildServiceProvider();
        _command = new(_logger);
        _context = new(_serviceProvider);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        var command = _command.GetCommand();
        Assert.Equal("get", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name amlfs-01 --job-name import-job-01", true)]
    [InlineData("--resource-group rg1 --filesystem-name amlfs-01", false)] // Missing subscription
    [InlineData("--subscription sub123 --filesystem-name amlfs-01", false)] // Missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // Missing filesystem
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            var importJob = new Models.ImportJob { Name = JobName, Id = "id1", ProvisioningState = "Succeeded" };

            _managedLustreService.GetImportJobAsync(
                Arg.Is(Sub), Arg.Is(Rg), Arg.Is(Name), Arg.Is(JobName),
                Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
                .Returns(importJob);
        }

        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(shouldSucceed ? HttpStatusCode.OK : HttpStatusCode.BadRequest, response.Status);
        if (shouldSucceed)
        {
            Assert.NotNull(response.Results);
            var json = JsonSerializer.Serialize(response.Results);
            var result = JsonSerializer.Deserialize(json, ManagedLustreJsonContext.Default.ImportJobGetResult);
            Assert.NotNull(result);
            Assert.Equal(JobName, result.Job.Name);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithJobName_CallsGetSingleJob()
    {
        // Arrange
        var importJob = new Models.ImportJob
        {
            Name = JobName,
            Id = "id1",
            ProvisioningState = "Succeeded",
            ConflictResolutionMode = "Fail",
            MaximumErrors = 10
        };

        _managedLustreService.GetImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(importJob);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name} --job-name {JobName}";
        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.DidNotReceive().ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithoutJobName_CallsListJobs()
    {
        // Arrange
        var importJobs = new List<Models.ImportJob>
        {
            new() { Name = "job1", Id = "id1", ProvisioningState = "Running" },
            new() { Name = "job2", Id = "id2", ProvisioningState = "Succeeded" }
        };

        _managedLustreService.ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(importJobs);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name}";
        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        _managedLustreService.ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Service error"));

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name}";
        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.Status);
        Assert.Contains("Service error", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        // Arrange
        var importJobs = new List<Models.ImportJob>
        {
            new() { Name = JobName, Id = "id1", ProvisioningState = "Succeeded" }
        };

        _managedLustreService.ListImportJobsAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(importJobs);

        var parseResult = _commandDefinition.Parse([
            "--subscription", Sub, "--resource-group", Rg, "--filesystem-name", Name]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ManagedLustreJsonContext.Default.ImportJobListResult);
        Assert.NotNull(result);
        Assert.Single(result!.Jobs);
        Assert.Equal(JobName, result.Jobs[0].Name);
    }
}
