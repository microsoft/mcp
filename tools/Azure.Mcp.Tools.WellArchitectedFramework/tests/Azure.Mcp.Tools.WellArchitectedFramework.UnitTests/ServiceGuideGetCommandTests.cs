// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.WellArchitectedFramework.UnitTests;

public class ServiceGuideGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceGuideGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly ServiceGuideGetCommand _command;
    private readonly Command _commandDefinition;

    public ServiceGuideGetCommandTests()
    {
        _logger = Substitute.For<ILogger<ServiceGuideGetCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.NotNull(_command);
        Assert.Equal("get", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Theory]
    [InlineData("--service app-service", true)]
    [InlineData("--service databricks", true)]
    [InlineData("--service sql-database", true)]
    [InlineData("--service cosmos-db", true)]
    [InlineData("", false)]
    [InlineData("--service", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.Equal("Missing Required options: --service", response.Message);
        }
    }

    [Theory]
    [InlineData("app-service")]
    [InlineData("databricks")]
    [InlineData("sql-database")]
    [InlineData("cosmos-db")]
    public async Task ExecuteAsync_ReturnsGuidance_WhenValidServiceProvided(string serviceName)
    {
        // Arrange
        var args = _commandDefinition.Parse($"--service {serviceName}");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);

        var serviceGuideUrlPrefix = "https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides";
        
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains($"For detailed Azure Well-Architected Framework guidance on {serviceName} service, please refer to the markdown file at this url:", result[0]);
        Assert.Contains(serviceGuideUrlPrefix, result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsPlaceholderGuidance_WhenServiceResourceNotFound()
    {
        // Arrange
        var serviceName = "non-existent-service-12345";
        var args = _commandDefinition.Parse($"--service {serviceName}");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert - Should return OK with placeholder guidance instead of error
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);
        
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains($"Azure Well-Architected Framework guidance for '{serviceName}' service is not available.", result[0]);
        Assert.Contains("https://learn.microsoft.com/azure/well-architected/service-guides", result[0]);
    }

    [Theory]
    [InlineData("app-service")]  // Exact match with hyphens
    [InlineData("appservice")]    // Without hyphens
    [InlineData("App-Service")]  // Mixed case with hyphens
    [InlineData("APPSERVICE")]    // Uppercase without hyphens
    [InlineData("sql-database")] // Another service with hyphens
    [InlineData("azuresql")]   // Different name
    [InlineData("sql")]   // Abbreviation
    public async Task ExecuteAsync_HandlesServiceNameVariations_Correctly(string serviceNameInput)
    {
        // Arrange
        var args = _commandDefinition.Parse($"--service {serviceNameInput}");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);
        
        Assert.NotNull(result);
        Assert.Single(result);
        
        // Should return guidance URL (not the "not available" message)
        Assert.Contains("For detailed Azure Well-Architected Framework guidance on", result[0]);
        Assert.Contains("please refer to the markdown file at this url:", result[0]);
        Assert.Contains("https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides", result[0]);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var serviceName = "app-service";
        var args = _commandDefinition.Parse($"--service {serviceName}");

        // Act
        var options = typeof(ServiceGuideGetCommand)
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, new object[] { args }) as Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide.ServiceGuideGetOptions;

        // Assert
        Assert.NotNull(options);
        Assert.Equal(serviceName, options.Service);
    }
}
