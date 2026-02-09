// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.ManagedLustre.Commands;
using Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.ManagedLustre.UnitTests.FileSystem.ImportJob;

public class ImportJobCreateCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IManagedLustreService _managedLustreService;
    private readonly ILogger<ImportJobCreateCommand> _logger;
    private readonly ImportJobCreateCommand _command;
    private readonly CommandContext _context;
    private readonly Command _commandDefinition;

    private const string Sub = "sub123";
    private const string Rg = "rg1";
    private const string Name = "amlfs-01";
    private const string JobName = "import-job-01";

    public ImportJobCreateCommandTests()
    {
        _managedLustreService = Substitute.For<IManagedLustreService>();
        _logger = Substitute.For<ILogger<ImportJobCreateCommand>>();
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
        Assert.Equal("create", command.Name);
        Assert.NotNull(command.Description);
        Assert.NotEmpty(command.Description);
    }

    [Theory]
    [InlineData("--subscription sub123 --resource-group rg1 --filesystem-name amlfs-01", true)]
    [InlineData("--resource-group rg1 --filesystem-name amlfs-01", false)] // Missing subscription
    [InlineData("--subscription sub123 --filesystem-name amlfs-01", false)] // Missing resource-group
    [InlineData("--subscription sub123 --resource-group rg1", false)] // Missing filesystem
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        if (shouldSucceed)
        {
            _managedLustreService.CreateImportJobAsync(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string?>(),
                Arg.Any<string?>(),
                Arg.Any<string[]?>(),
                Arg.Any<long?>(),
                Arg.Any<string?>(),
                Arg.Any<RetryPolicyOptions?>(),
                Arg.Any<CancellationToken>())
                .Returns(JobName);
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
            var result = JsonSerializer.Deserialize(json, ManagedLustreJsonContext.Default.ImportJobCreateResult);
            Assert.NotNull(result);
            Assert.Equal(JobName, result!.JobName);
        }
        else
        {
            Assert.Contains("required", response.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ExecuteAsync_WithOptionalParameters_CallsService()
    {
        // Arrange
        const string jobName = "custom-job";
        const string conflictMode = "Fail";
        const string prefixes = "folder1/,folder2/";
        const long maxErrors = 10;

        _managedLustreService.CreateImportJobAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string?>(),
            Arg.Any<string[]?>(),
            Arg.Any<long?>(),
            Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(),
            Arg.Any<CancellationToken>())
            .Returns(jobName);

        var args = $"--subscription {Sub} --resource-group {Rg} --filesystem-name {Name} --job-name {jobName} --conflict-resolution-mode {conflictMode} --import-prefixes {prefixes} --maximum-errors {maxErrors}";
        var parseResult = _commandDefinition.Parse(args.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        await _managedLustreService.Received(1).CreateImportJobAsync(
            Sub, Rg, Name, jobName, conflictMode,
            Arg.Any<string[]>(),
            Arg.Any<long?>(), Arg.Any<string?>(), Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsException_ReturnsErrorResponse()
    {
        // Arrange
        _managedLustreService.CreateImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<long?>(), Arg.Any<string?>(),
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
        _managedLustreService.CreateImportJobAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(),
            Arg.Any<string?>(), Arg.Any<string[]?>(), Arg.Any<long?>(), Arg.Any<string?>(),
            Arg.Any<RetryPolicyOptions?>(), Arg.Any<CancellationToken>())
            .Returns(JobName);

        var parseResult = _commandDefinition.Parse([
            "--subscription", Sub, "--resource-group", Rg, "--filesystem-name", Name]);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, ManagedLustreJsonContext.Default.ImportJobCreateResult);
        Assert.NotNull(result);
        Assert.Equal(JobName, result!.JobName);
    }
}
