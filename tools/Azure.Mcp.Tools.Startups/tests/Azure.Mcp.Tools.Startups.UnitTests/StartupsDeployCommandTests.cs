// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.CommandLine.Parsing;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.Startups.Commands;
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

    public StartupsDeployCommandTests()
    {
        _startupsService = Substitute.For<IStartupsService>();
        _logger = Substitute.For<ILogger<StartupsDeployCommand>>();

        var collection = new ServiceCollection();
        collection.AddSingleton(_startupsService);
        _serviceProvider = collection.BuildServiceProvider();

        _command = new(_logger);

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
        Assert.Contains("storage-account", options);
        Assert.Contains("resource-group", options);
        Assert.Contains("source-path", options);
        Assert.Contains("subscription", options);
        Assert.Contains("tenant", options);
        Assert.Contains("overwrite", options);
    }

    [Fact]
    public async Task ExecuteAsync_ValidStaticWebDeployment_ReturnsSuccess()
    {
        // Arrange
        var htmlFile = Path.Combine(_tempDirectory, "index.html");
        File.WriteAllText(htmlFile, "<html><body></body></html>");

        var expectedResult = new StartupsDeployResources(
            StorageAccount: "",
            Container: "$web",
            Status: "Success",
            WebsiteUrl: "https://teststorage.z22.web.core.windows.net/",
            PortalUrl: "https:/teststorage.azure.com/...",
            ContainerUrl: "https://teststorage.azure.com/..."
        );

        _startupsService.DeployStaticWebAsync(
            Arg.Any<string>(), // tenantId
            Arg.Any<string>(), // subscription
            Arg.Any<string>(), // storageAccount
            Arg.Any<string>(), // resourceGroup
            Arg.Any<string>(), // sourcePath
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<bool>(), // overwrite
            Arg.Any<IProgress<string>>()
        ).Returns(expectedResult);

        var args = $"--storage-account teststorage --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123";
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);
        Assert.Equal("Success", response.Message);
        Assert.NotNull(response.Results);

        var result = JsonSerializer.Deserialize<StartupsDeployResources>(
            JsonSerializer.Serialize(response.Results));
        Assert.NotNull(result);
        Assert.Equal("teststorage", result.StorageAccount);
        Assert.Equal("$web", result.Container);
        Assert.Contains("https://", result.WebsiteUrl);
    }

    [Fact]
    public async Task ExecuteAsync_ReactProject_AutoDetectsAndDeploys()
    {
        // Arrange
        var packageJsonContent = """
        {
          "name": "test-react-app",
          "dependencies": {
            "react": "^18.0.0"
          },
          "scripts": {
            "build": "react-scripts build"
          }
        }
        """;

        var packageJsonFile = Path.Combine(_tempDirectory, "package.json");
        File.WriteAllText(packageJsonFile, packageJsonContent);

        var buildDir = Path.Combine(_tempDirectory, "build");
        Directory.CreateDirectory(buildDir);
        File.WriteAllText(Path.Combine(buildDir, "index.html"), "<html>React App</html>");

        var expectedResult = new StartupsDeployResources(
            StorageAccount: "reactstorage",
            Container: "$web",
            Status: "Success",
            WebsiteUrl: "https://reactstorage.z22.web.core.windows.net/",
            PortalUrl: "https://portal.azure.com/...",
            ContainerUrl: "https://portal.azure.com/..."
        );

        _startupsService.DeployStaticWebAsync(
            Arg.Any<string>(), // tenantId
            Arg.Any<string>(), // subscription
            Arg.Any<string>(), // storageAccount
            Arg.Any<string>(), // resourceGroup
            Arg.Any<string>(), // sourcePath
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<bool>(), // overwrite
            Arg.Any<IProgress<string>>()
        ).Returns(expectedResult);

        var args = $"--storage-account reactstorage --resource-group test-rg --source-path {_tempDirectory} --subscription sub123 --tenant tenant123";
        var context = new CommandContext(_serviceProvider);
        var parseResult = _command.GetCommand().Parse(args);

        // Act
        var response = await _command.ExecuteAsync(context, parseResult);

        // Assert
        Assert.Equal(200, response.Status);

        // Verify service was called
        await _startupsService.Received(1).DeployStaticWebAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            "reactstorage",
            "test-rg",
            Arg.Any<string>(),
            Arg.Any<RetryPolicyOptions>(),
            Arg.Any<bool>(),
            Arg.Any<IProgress<string>>()
        );
    }
    // validation tests
    // required options missing
    // invalid storage account naming
    // azure services
    // overwrite
}
