using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using ToolMetadataExporter.Models;
using ToolMetadataExporter.Services;
using Xunit;

namespace ToolMetadataExporter.UnitTests.Services;

public class AzmcpProgramTests
{
    private readonly Utility _utility;
    private readonly ILogger<AzmcpProgram> _logger;
    private readonly IOptions<AppConfiguration> _options;
    private readonly AppConfiguration _appConfiguration;

    public AzmcpProgramTests()
    {
        var utilityLogger = Substitute.For<ILogger<Utility>>();
        _utility = Substitute.For<Utility>(utilityLogger);

        _logger = Substitute.For<ILogger<AzmcpProgram>>();
        _options = Substitute.For<IOptions<AppConfiguration>>();

        _appConfiguration = new AppConfiguration
        {
            AzmcpExe = "azmcp",
            WorkDirectory = "/tmp"
        };

        _options.Value.Returns(_appConfiguration);
    }

    /// <summary>
    /// Verifies that the server name is correctly retrieved from the server information response and is lowercase.
    /// </summary>
    [Fact]
    public async Task GetsServerNameWithServerInfo()
    {
        // Arrange
        var serverInfo = new ServerInfo { Name = "Template.Mcp.Server", Version = "1.0.0-beta.1+20-0-2" };
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = serverInfo
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.GetServerNameAsync();

        Assert.Equal(serverInfo.Name.ToLowerInvariant(), actual);
    }

    /// <summary>
    /// Verifies that the server name is correctly retrieved from the help command output when server info fails.
    /// Checks that the name is formatted to lowercase with dots replacing spaces.
    /// </summary>
    [Fact]
    public async Task GetsServerNameWithHelp()
    {
        // Arrange
        var serverInfoOutput =
            """
            Required command was not provided.
            Unrecognized command or argument 'info'.

            Description:
              MCP Server operations - Commands for managing and interacting with the MCP Server.

            Usage:
              azmcp server [command] [options]

            Options:
              -?, -h, --help  Show help and usage information
            """;
        var helpOutput = """
            Description:
              Template MCP Server

            Usage:
              azmcp [command] [options]

            Options:
              -?, -h, --help  Show help and usage information
              --version       Show version information

            Commands:
              get_bestpractices            Azure best practices - Commands return a list of best practices for code generation,
            """;

        _utility.ExecuteAzmcpAsync(Arg.Any<string>(), "server info", checkErrorCode: false).Returns(serverInfoOutput);
        _utility.ExecuteAzmcpAsync(Arg.Any<string>(), "--help", checkErrorCode: false).Returns(helpOutput);

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.GetServerNameAsync();

        Assert.Equal("Template.MCP.Server", actual);
    }

    [Fact]
    public async Task GetsServerVersionFromServerInfo()
    {
        // Arrange
        var serverInfo = new ServerInfo { Name = "Azure.Mcp.Server", Version = "1.2.3-beta.4" };
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = serverInfo
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.GetServerVersionAsync();

        // Assert
        Assert.Equal("1.2.3-beta.4", actual);
    }

    [Fact]
    public async Task GetsServerVersionFromVersionCommand()
    {
        // Arrange
        var serverInfoOutput = "invalid json";
        var versionOutput = "2.0.1";

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serverInfoOutput));
        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "--version", checkErrorCode: false).Returns(Task.FromResult(versionOutput));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.GetServerVersionAsync();

        // Assert
        Assert.Equal("2.0.1", actual);
    }

    [Fact]
    public async Task GetsServerVersionStripsGitHash()
    {
        // Arrange
        var serverInfoOutput = "invalid json";
        var versionOutput = "1.5.0+abc123def456";

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serverInfoOutput));
        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "--version", checkErrorCode: false).Returns(Task.FromResult(versionOutput));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.GetServerVersionAsync();

        // Assert
        Assert.Equal("1.5.0", actual);
    }

    [Fact]
    public async Task LoadToolsDynamicallyAsync_CallsUtilityMethod()
    {
        // Arrange
        var serverInfo = new ServerInfo { Name = "Test.Server", Version = "1.0.0" };
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = serverInfo
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.LoadToolsDynamicallyAsync();

        // Assert - Verify the utility method was called
        await _utility.Received(1).LoadToolsDynamicallyAsync(_appConfiguration.AzmcpExe!, _appConfiguration.WorkDirectory!, false);
    }

    [Fact]
    public void Constructor_ThrowsWhenWorkDirectoryIsNull()
    {
        // Arrange
        var invalidConfig = new AppConfiguration
        {
            AzmcpExe = "azmcp",
            WorkDirectory = null
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AzmcpProgram(_utility, invalidOptions, _logger));
    }

    [Fact]
    public void Constructor_ThrowsWhenAzmcpExeIsNull()
    {
        // Arrange
        var invalidConfig = new AppConfiguration
        {
            AzmcpExe = null,
            WorkDirectory = "/tmp"
        };
        var invalidOptions = Substitute.For<IOptions<AppConfiguration>>();
        invalidOptions.Value.Returns(invalidConfig);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AzmcpProgram(_utility, invalidOptions, _logger));
    }

    [Fact]
    public async Task GetServerNameAsync_ThrowsWhenBothServerInfoAndHelpFail()
    {
        // Arrange
        var serverInfoOutput = "invalid json";
        var helpOutput = """
            Usage:
              azmcp [command] [options]

            Options:
              -?, -h, --help  Show help and usage information
            """;

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serverInfoOutput));
        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "--help", checkErrorCode: false).Returns(Task.FromResult(helpOutput));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await program.GetServerNameAsync());
        Assert.Contains("Failed to determine server name", exception.Message);
    }

    [Fact]
    public async Task GetServerInfoInternal_ReturnsNullForNullResults()
    {
        // Arrange
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = null
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);
        var helpOutput = """
            Description:
              Null Results Server

            Usage:
              azmcp [command] [options]
            """;

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));
        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "--help", checkErrorCode: false).Returns(Task.FromResult(helpOutput));

        // Act
        var program = new AzmcpProgram(_utility, _options, _logger);
        var actual = await program.GetServerNameAsync();

        // Assert
        Assert.Equal("Null.Results.Server", actual);
    }

    [Fact]
    public async Task GetServerVersionAsync_HandlesVersionWithWhitespace()
    {
        // Arrange
        var serverInfoOutput = "invalid json";
        var versionOutput = "  3.2.1  \n";

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serverInfoOutput));
        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "--version", checkErrorCode: false).Returns(Task.FromResult(versionOutput));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act
        var actual = await program.GetServerVersionAsync();

        // Assert
        Assert.Equal("3.2.1", actual);
    }

    [Fact]
    public async Task GetServerNameAsync_CachesResult()
    {
        // Arrange
        var serverInfo = new ServerInfo { Name = "Cached.Server", Version = "1.0.0" };
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = serverInfo
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act - Call twice
        var firstResult = await program.GetServerNameAsync();
        var secondResult = await program.GetServerNameAsync();

        // Assert
        Assert.Equal("cached.server", firstResult);
        Assert.Equal(firstResult, secondResult);

        // Verify ExecuteAzmcpAsync was only called once for server info (during construction)
        await _utility.Received(1).ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false);
    }

    [Fact]
    public async Task GetServerVersionAsync_CachesResult()
    {
        // Arrange
        var serverInfo = new ServerInfo { Name = "Test.Server", Version = "5.6.7" };
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = serverInfo
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act - Call twice
        var firstResult = await program.GetServerVersionAsync();
        var secondResult = await program.GetServerVersionAsync();

        // Assert
        Assert.Equal("5.6.7", firstResult);
        Assert.Equal(firstResult, secondResult);

        // Verify ExecuteAzmcpAsync was only called once for server info (during construction)
        await _utility.Received(1).ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false);
    }

    [Fact]
    public async Task LoadToolsDynamicallyAsync_UsesLazyInitialization()
    {
        // Arrange
        var serverInfo = new ServerInfo { Name = "Test.Server", Version = "1.0.0" };
        var serverInfoResult = new ServerInfoResult
        {
            Status = 200,
            Message = "Success",
            Results = serverInfo
        };
        var serialized = JsonSerializer.Serialize(serverInfoResult, ModelsSerializationContext.Default.ServerInfoResult);

        _utility.ExecuteAzmcpAsync(_appConfiguration.AzmcpExe!, "server info", checkErrorCode: false).Returns(Task.FromResult(serialized));

        var program = new AzmcpProgram(_utility, _options, _logger);

        // Act - Call twice to verify lazy initialization
        var firstResult = await program.LoadToolsDynamicallyAsync();
        var secondResult = await program.LoadToolsDynamicallyAsync();

        // Assert - Verify LoadToolsDynamicallyAsync was only called once (lazy initialization)
        await _utility.Received(1).LoadToolsDynamicallyAsync(_appConfiguration.AzmcpExe!, _appConfiguration.WorkDirectory!, false);
    }
}
