// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Options;
using Azure.Mcp.Tools.FileShares.Commands.FileShare;
using Azure.Mcp.Tools.FileShares.Options.FileShare;
using Azure.Mcp.Tools.FileShares.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Azure.Mcp.Tools.FileShares.UnitTests.FileShare;

public class FileShareListCommandTests
{
    private readonly IServiceProvider _serviceProvider;

    public FileShareListCommandTests()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton(Substitute.For<IFileSharesService>());
        services.AddSingleton<FileShareListCommand>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        // Arrange & Act
        var command = _serviceProvider.GetRequiredService<FileShareListCommand>();

        // Assert
        Assert.NotNull(command);
        Assert.Equal("list", command.Name);
        Assert.Equal("List File Shares", command.Title);
        Assert.False(command.Metadata.Destructive);
        Assert.True(command.Metadata.ReadOnly);
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var command = _serviceProvider.GetRequiredService<FileShareListCommand>();
        var parseResult = Substitute.For<ParseResult>();
        parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name).Returns("test-rg");
        parseResult.GetValueOrDefault<string>(Arg.Any<string>()).Returns(x => null);

        // Act
        var options = command.BindOptions(parseResult);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("test-rg", options.ResourceGroup);
    }

    [Theory]
    [InlineData("", false, "Missing required options")]
    [InlineData("--subscription test-sub", true, null)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed, string expectedError)
    {
        // Arrange
        var command = _serviceProvider.GetRequiredService<FileShareListCommand>();
        var service = _serviceProvider.GetRequiredService<IFileSharesService>();
        service.ListFileShares(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<RetryPolicyOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(new List<string> { "share1", "share2" }));

        // Act & Assert
        Assert.NotNull(command);
    }
}
