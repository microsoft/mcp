// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using NSubstitute.ReturnsExtensions;
using NSubstitute.ExceptionExtensions;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Startups.Commands;
using Azure.Mcp.Tools.Startups.Options;
using Azure.Mcp.Tools.Startups.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Startups.UnitTests;

[Trait("Area", "Startups")]
[Trait("Category", "Unit")]
public sealed class StartupsDeployCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStartupsService _startupsService;
    private readonly ILogger<StartupsDeployCommand> _logger;
    private readonly StartupsDeployCommand _command;
    private readonly string _tempDirectory;
    private readonly CommandContext _context;
    private readonly Parser _parser;

    public StartupsDeployCommandTests()
    {
        _startupsService = Substitute.For<IStartupsService>();
        _logger = Substitute.For<ILogger<StartupsDeployCommand>>();

        var collection = new ServiceCollection()
            .AddSingleton(_startupsService)
            .AddSingleton<ILogger<StartupsDeployCommand>>(_logger);

        _serviceProvider = collection.BuildServiceProvider();
        _command = new(_logger);
        _context = new CommandContext(_serviceProvider);
        _parser = new Parser(_command.GetCommand());

        // Create temp directory for test files
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void GetCommand_HasRequiredOptions()
    {
        // Act
        var command = _command.GetCommand();
        var options = command.Options.Select(o => o.Name).ToList();

        // Assert
        // Check command-specific required options
        Assert.Contains("storage-account", options);
        Assert.Contains("source-path", options);
        Assert.Contains("overwrite", options);
        
        // Check inherited required options from SubscriptionCommand
        Assert.Contains("subscription", options);
        Assert.Contains("tenant", options);
        
        // Check other expected options
        Assert.Contains("auth-method", options);
    }

    [Fact]
    public async Task ExecuteAsync_ValidStaticWebDeployment_ReturnsSuccess()
    {
        // Arrange
        var htmlFile = Path.Combine(_tempDirectory, "index.html");
        File.WriteAllText(htmlFile, "<html><body>Test content</body></html>");

        var expectedResult = new StartupsDeployResources(
            StorageAccount: "teststorage",
            Container: "$web",
            Status: "Success",
            WebsiteUrl: "https://teststorage.z22.web.core.windows.net/",
            PortalUrl: "https:/teststorage.azure.com/...",
            ContainerUrl: "https://teststorage.azure.com/..."
        );

        _startupsService.DeployStaticWebAsync(
            Arg.Any<StartupsDeployOptions>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<IProgress<string>>()
        ).Returns(expectedResult);

        var args = $"--storage-account teststorage --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123 --overwrite false";
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        // Verify service was called with correct parameters
        await _startupsService.Received(1).DeployStaticWebAsync(
            Arg.Is<StartupsDeployOptions>(opts => 
                opts.Tenant == "tenant123" &&
                opts.Subscription == "sub123" &&
                opts.StorageAccount == "teststorage" &&
                opts.ResourceGroup == "test-rg" &&
                opts.SourcePath == _tempDirectory &&
                !opts.Overwrite),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<IProgress<string>>());

        var result = JsonSerializer.Deserialize<StartupsDeployResources>(
            JsonSerializer.Serialize(response.Results));
        Assert.NotNull(result);
        Assert.Equal("teststorage", result.StorageAccount);
        Assert.Equal("$web", result.Container);
        Assert.Contains("https://", result.WebsiteUrl);
    }

    [Fact]
    public async Task ExecuteAsync_WithOverwriteEnabled_DeploysWithOverwrite()
    {
        // Arrange
        var htmlFile = Path.Combine(_tempDirectory, "index.html");
        File.WriteAllText(htmlFile, "<html><body>Test content</body></html>");

        var expectedResult = new StartupsDeployResources(
            StorageAccount: "teststorage",
            Container: "$web",
            Status: "Success",
            WebsiteUrl: "https://teststorage.z22.web.core.windows.net/",
            PortalUrl: "https:/teststorage.azure.com/...",
            ContainerUrl: "https://teststorage.azure.com/..."
        );

        _startupsService.DeployStaticWebAsync(
            Arg.Any<StartupsDeployOptions>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<IProgress<string>>()
        ).Returns(expectedResult);

        var args = $"--storage-account teststorage --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123 --overwrite true";
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);

        // Verify overwrite flag was passed correctly
        await _startupsService.Received(1).DeployStaticWebAsync(
            Arg.Is<StartupsDeployOptions>(opts => opts.Overwrite),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<IProgress<string>>());
    }

    [Fact]
    public async Task ExecuteAsync_InvalidSourcePath_ThrowsError()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "does-not-exist");
        var args = $"--storage-account teststorage --resource-group test-rg --source-path {nonExistentPath} --subscription sub123 --tenant tenant123 --overwrite false";
        var parseResult = _command.GetCommand().Parse(args);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _command.ExecuteAsync(_context, parseResult));
        Assert.Contains("Source path does not exist", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_EmptySourceDirectory_ThrowsError()
    {
        // Arrange
        var args = $"--storage-account teststorage --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123 --overwrite false";
        var parseResult = _command.GetCommand().Parse(args);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _command.ExecuteAsync(_context, parseResult));
        Assert.Contains("Source directory is empty", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrowsError_ReturnsError()
    {
        // Arrange
        var htmlFile = Path.Combine(_tempDirectory, "index.html");
        File.WriteAllText(htmlFile, "<html><body>Test content</body></html>");

        var errorMessage = "Failed to upload files to storage account: Access denied";
        _startupsService.DeployStaticWebAsync(
            Arg.Any<StartupsDeployOptions>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<IProgress<string>>()
        ).ThrowsAsync(new Exception(errorMessage));

        var args = $"--storage-account teststorage --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123 --overwrite false";
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult);

        // Assert
        Assert.Equal(500, response.Status);
        Assert.Contains(errorMessage, response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidStorageAccountName_ThrowsError()
    {
        // Arrange
        var htmlFile = Path.Combine(_tempDirectory, "index.html");
        File.WriteAllText(htmlFile, "<html><body>Test content</body></html>");

        // Storage account names must be between 3 and 24 characters and use only lowercase letters and numbers
        var args = $"--storage-account Test-Storage! --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123 --overwrite false";
        var parseResult = _command.GetCommand().Parse(args);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _command.ExecuteAsync(_context, parseResult));
        Assert.Contains("Invalid storage account name", exception.Message);
    }
}
