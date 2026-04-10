// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.WellArchitectedFramework.Commands.ServiceGuide;
using Azure.Mcp.Tools.WellArchitectedFramework.Services.ServiceGuide;
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
    private readonly IServiceGuideService _serviceGuideService;
    private readonly CommandContext _context;
    private readonly ServiceGuideGetCommand _command;
    private readonly Command _commandDefinition;

    public ServiceGuideGetCommandTests()
    {
        _logger = Substitute.For<ILogger<ServiceGuideGetCommand>>();
        _serviceGuideService = new ServiceGuideService();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger, _serviceGuideService);
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
    [InlineData("--service \"app service\"", true)]
    [InlineData("--service cosmos-db", true)]
    [InlineData("", true)]  // Empty args should succeed and return list of services
    [InlineData("--service", false)]  // --service without value should fail
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Assert
        if (shouldSucceed)
        {
            // Act
            var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
        }
        else
        {
            // For failure cases, check if there are parse errors first
            if (parseResult.Errors.Count > 0)
            {
                // Parse-time validation errors (option without value)
                Assert.True(parseResult.Errors.Count > 0);
                Assert.Contains("--service", parseResult.Errors.Select(e => e.Message).FirstOrDefault() ?? string.Empty);
            }
            else
            {
                // Act
                var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

                // Runtime validation errors
                Assert.NotEqual(HttpStatusCode.OK, response.Status);
                Assert.Contains("'--service'", response.Message);
            }
        }
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsServiceList_WhenNoServiceProvided()
    {
        // Arrange
        var args = _commandDefinition.Parse("");

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
        Assert.Contains("Azure Well-Architected Framework service guides are available for the following services:", result[0]);
        Assert.Contains("To get guidance for a specific service, use this command with the --service <service-name> option", result[0]);
        // Should contain at least some common service names
        Assert.Contains("app-service", result[0]);
    }

    [Theory]
    [InlineData("app-service")]
    [InlineData("databricks")]
    [InlineData("\"app service\"")]
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
        // Check for the key parts of the multi-line response
        Assert.Contains($"For detailed Azure Well-Architected Framework guidance on", result[0]);
        Assert.Contains("please refer to the markdown file at this URL:", result[0]);
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
        Assert.Contains("For more information, visit: https://learn.microsoft.com/azure/well-architected/service-guides", result[0]);
    }

    [Theory]
    [InlineData("azure-sql-database")]  // Exact match with hyphens
    [InlineData("Azure-SQL-Database")]  // Case insensitive
    [InlineData("azure_sql_database")]  // With underscores
    [InlineData("azuresqldatabase")]    // Without hyphens, underscores, or spaces
    [InlineData("\"azure sql database\"")]  // With spaces. Double quotes needed
    [InlineData("\"azure-sql-database\"")]  // Extra double quotes to test trimming
    [InlineData("'azure-sql-database'")]  // Extra single quotes to test trimming
    [InlineData("sql-database")] // Another service name variation, with same checks as above
    [InlineData("SQL-Database")]
    [InlineData("sql_database")]
    [InlineData("SQLDATABASE")]
    [InlineData("\"sql database\"")]
    [InlineData("\"sql-database\"")]
    [InlineData("'sql-database'")]
    [InlineData("sql-db")] // Another service name variation, with same checks as above
    [InlineData("SQL-DB")]
    [InlineData("sql_db")]
    [InlineData("SQLDB")]
    [InlineData("\"sql db\"")]
    [InlineData("\"sql-db\"")]
    [InlineData("'sql-db'")]
    public async Task ExecuteAsync_HandlesServiceNameVariationsNormalized_Correctly(string serviceNameInput)
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
        Assert.Contains("please refer to the markdown file at this URL:", result[0]);
        // azure-sql-database.md should be present in the URL regardless of input variation
        Assert.Contains("https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/azure-sql-database.md", result[0]);
    }

    /// <summary>
    /// Tests how the command handles different quote styles around service names.
    /// Key findings:
    /// - Double quotes ("...") are properly handled by the System.CommandLine parser and are stripped from the value
    /// - Single quotes ('...') are treated as literal characters and become part of the actual value. This is the standard .NET command-line parsing behavior.
    /// - Single quotes with spaces create unparsed tokens that cause validation errors (BadRequest)
    /// - This test documents the actual behavior of the command-line parser
    /// </summary>
    [Theory]
    [InlineData("app-service", "app-service", true)]  // No quotes, no spaces
    [InlineData("\"app-service\"", "app-service", true)]  // Double quotes, no spaces - stripped by parser
    [InlineData("\"app service\"", "app service", true)]  // Double quotes with spaces - stripped by parser
    [InlineData("\"Azure App Service\"", "Azure App Service", true)]  // Double quotes with spaces and mixed case
    [InlineData("'app-service'", "'app-service'", true)]  // Single quotes, no spaces - preserved as literals
    [InlineData("'app service'", "'app", false)]  // Single quotes with spaces - parser error due to unparsed token 'service'
    [InlineData("'Azure App Service'", "'Azure", false)]  // Single quotes with spaces - parser error due to unparsed tokens 'App Service'
    public async Task ExecuteAsync_HandlesQuotedServiceNames_Correctly(string inputServiceName, string expectedServiceName, bool shouldSucceed)
    {
        // Arrange
        var args = _commandDefinition.Parse($"--service {inputServiceName}");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        
        if (shouldSucceed)
        {
            // Verify the command executes successfully
            Assert.Equal(HttpStatusCode.OK, response.Status);
            Assert.NotNull(response.Results);
            
            // Verify the service name was parsed correctly by checking the bound options
            var options = typeof(ServiceGuideGetCommand)
                .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_command, new object[] { args }) as Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide.ServiceGuideGetOptions;

            Assert.NotNull(options);
            Assert.Equal(expectedServiceName, options.Service);
        }
        else
        {
            // Single quotes with spaces create unparsed tokens that cause validation errors
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
            Assert.NotNull(response.Message);
        }
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly_WhenNoServiceProvided()
    {
        // Arrange
        var args = _commandDefinition.Parse("");

        // Act
        var options = typeof(ServiceGuideGetCommand)
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, new object[] { args }) as Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide.ServiceGuideGetOptions;

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.Service);
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
