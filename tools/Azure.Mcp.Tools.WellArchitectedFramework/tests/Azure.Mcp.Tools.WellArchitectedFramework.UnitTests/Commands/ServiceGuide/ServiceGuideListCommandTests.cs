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

public class ServiceGuideListCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceGuideListCommand> _logger;
    private readonly IServiceGuideService _serviceGuideService;
    private readonly CommandContext _context;
    private readonly ServiceGuideListCommand _command;
    private readonly Command _commandDefinition;

    public ServiceGuideListCommandTests()
    {
        _logger = Substitute.For<ILogger<ServiceGuideListCommand>>();
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
        Assert.Equal("list", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.False(_command.Metadata.Destructive);
        Assert.True(_command.Metadata.Idempotent);
        Assert.False(_command.Metadata.OpenWorld);
        Assert.True(_command.Metadata.ReadOnly);
        Assert.False(_command.Metadata.LocalRequired);
        Assert.False(_command.Metadata.Secret);
    }

    [Theory]
    [InlineData("--services app-service")]
    [InlineData("--services app-service cosmos-db")]
    [InlineData("--services databricks \"sql database\"")]
    public async Task ExecuteAsync_ValidatesInputCorrectly_WithValidInput(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Theory]
    [InlineData("")]
    [InlineData("--services")]
    public async Task ExecuteAsync_ValidatesInputCorrectly_WithInvalidInput(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("Missing Required options: --services", response.Message);
    }

    [Fact]
    public async Task ExecuteAsync_HandlesSingleService_Correctly()
    {
        // Arrange
        var args = _commandDefinition.Parse("--services app-service");

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
        Assert.Contains("app-service: https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/app-service-web-apps.md", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGuidance_WhenAllServicesAreValid()
    {
        // Arrange
        var args = _commandDefinition.Parse("""--services app-service "sql database" cosmos-db""");

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
        Assert.Single(result); // Only one section since all services have guidance
        Assert.Contains("For detailed Azure Well-Architected Framework guidance", result[0]);
        // List command outputs service names as provided by user, followed by colon
        Assert.Contains("app-service:", result[0]);
        Assert.Contains("sql database:", result[0]); // Uses input name, not normalized
        Assert.Contains("cosmos-db:", result[0]);
        Assert.Contains(serviceGuideUrlPrefix, result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsGuidanceNotAvailable_WhenAllServicesAreInvalid()
    {
        // Arrange
        var args = _commandDefinition.Parse("--services non-existent-service-1 non-existent-service-2");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);
        
        Assert.NotNull(result);
        Assert.Single(result); // Only one section since no services have guidance
        Assert.Contains("Azure Well-Architected Framework guidance for the following service(s) is not available:", result[0]);
        Assert.Contains("'non-existent-service-1'", result[0]);
        Assert.Contains("'non-existent-service-2'", result[0]);
        Assert.Contains("Supported services include:", result[0]);
        Assert.Contains("https://learn.microsoft.com/azure/well-architected/service-guides", result[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsMixedResults_WhenSomeServicesAreValidAndSomeAreNot()
    {
        // Arrange
        var args = _commandDefinition.Parse("--services app-service non-existent-service sql-database");

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);
        
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); // Two sections: services with guidance and services without

        // First result should contain valid services with URLs
        Assert.Contains("For detailed Azure Well-Architected Framework guidance", result[0]);
        Assert.Contains("app-service:", result[0]);
        Assert.Contains("sql-database:", result[0]);
        Assert.Contains("https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides", result[0]);

        // Second result should contain invalid services
        Assert.Contains("Azure Well-Architected Framework guidance for the following service(s) is not available:", result[1]);
        Assert.Contains("'non-existent-service'", result[1]);
        Assert.Contains("Supported services include:", result[1]);
    }

    [Theory]
    [InlineData("--services azure-sql-database azure-vms")]  // Exact match with hyphens
    [InlineData("--services Azure-SQL-Database Azure-Vms")]  // Case insensitive
    [InlineData("--services azure_sql_database azure_vms")]  // With underscores
    [InlineData("--services azuresqldatabase azurevms")]    // Without hyphens, underscores, or spaces
    [InlineData("--services \"azure sql database\" \"azure vms\"")]  // With spaces. Double quotes needed
    [InlineData("--services \"azure-sql-database\" \"azure-vms\"")]  // Extra double quotes to test trimming
    [InlineData("--services 'azure-sql-database' 'azure-vms'")]  // Extra single quotes to test trimming
    [InlineData("--services sql-database virtual-machines")] // Another service name variation
    [InlineData("--services SQL-Database Virtual-Machines")]
    [InlineData("--services sql_database virtual_machines")]
    [InlineData("--services SQLDATABASE VIRTUALMACHINES")]
    [InlineData("--services \"sql database\" \"virtual machines\"")]
    [InlineData("--services \"sql-database\" \"virtual-machines\"")]
    [InlineData("--services 'sql-database' 'virtual-machines'")]
    [InlineData("--services sql-db vms")] // Another service name variation
    [InlineData("--services SQL-DB VMS")]
    [InlineData("--services sql_db vms")]
    [InlineData("--services SQLDB VMS")]
    [InlineData("--services \"sql db\" vms")]
    [InlineData("--services \"sql-db\" vms")]
    [InlineData("--services 'sql-db' vms")]
    public async Task ExecuteAsync_HandlesServiceNameVariationsNormalized_Correctly(string args)
    {
        // Arrange
        var parseResult = _commandDefinition.Parse(args);

        // Act
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<List<string>>(json);
        
        Assert.NotNull(result);
        Assert.Single(result);
        
        // All variations should resolve to valid services with guidance URLs
        Assert.Contains("For detailed Azure Well-Architected Framework guidance", result[0]);
        // azure-sql-database.md should be present in the URL regardless of input variation
        Assert.Contains("https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/azure-sql-database.md", result[0]);
        // virtual-machines.md should be present in the URL regardless of input variation
        Assert.Contains("https://raw.githubusercontent.com/MicrosoftDocs/well-architected/main/well-architected/service-guides/virtual-machines.md", result[0]);
    }

    /// <summary>
    /// Tests how the command handles different quote styles around service names when processing multiple services.
    /// Key findings:
    /// - Double quotes ("...") are properly handled by the System.CommandLine parser and are stripped from the value
    /// - Single quotes ('...') are treated as literal characters and become part of the actual value. This is the standard .NET command-line parsing behavior.
    /// - For array arguments, each space-separated value is a separate element
    /// - This test documents the actual behavior of the command-line parser for array options
    /// </summary>
    [Theory]
    [InlineData("--services app-service sql-database", new[] { "app-service", "sql-database" })]  // No quotes, no spaces
    [InlineData("--services \"app-service\" \"sql-database\"", new[] { "app-service", "sql-database" })]  // Double quotes, no spaces - stripped by parser
    [InlineData("--services \"app service\" \"sql database\"", new[] { "app service", "sql database" })]  // Double quotes with spaces - stripped by parser
    [InlineData("--services \"Azure App Service\" \"Azure SQL Database\"", new[] { "Azure App Service", "Azure SQL Database" })]  // Double quotes with spaces and mixed case
    [InlineData("--services 'app-service' 'sql-database'", new[] { "'app-service'", "'sql-database'" })]  // Single quotes, no spaces - preserved as literals
    public async Task ExecuteAsync_HandlesQuotedServiceNames_Correctly(string inputArgs, string[] expectedServiceNames)
    {
        // Arrange
        var args = _commandDefinition.Parse(inputArgs);

        // Act
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(response);
        
        // Verify the command executes successfully
        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
        
        // Verify the service names were parsed correctly by checking the bound options
        var options = typeof(ServiceGuideListCommand)
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, new object[] { args }) as Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide.ServiceGuideListOptions;

        Assert.NotNull(options);
        Assert.NotNull(options.Services);
        Assert.Equal(expectedServiceNames.Length, options.Services.Length);
        Assert.Equal(expectedServiceNames, options.Services);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        // Arrange
        var serviceNames = new[] { "app-service", "sql-database", "cosmos-db" };
        var args = _commandDefinition.Parse($"--services {string.Join(" ", serviceNames)}");

        // Act
        var options = typeof(ServiceGuideListCommand)
            .GetMethod("BindOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_command, new object[] { args }) as Azure.Mcp.Tools.WellArchitectedFramework.Options.ServiceGuide.ServiceGuideListOptions;

        // Assert
        Assert.NotNull(options);
        Assert.NotNull(options.Services);
        Assert.Equal(serviceNames.Length, options.Services.Length);
        Assert.Equal(serviceNames, options.Services);
    }
}
