using System.CommandLine;
using System.Text.Json;
using Azure.Mcp.Core.Models.Command;
using Azure.Mcp.Tools.AzureTerraformBestPractices.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraformBestPractices.UnitTests;

public class AzureTerraformBestPracticesGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AzureTerraformBestPracticesGetCommand> _logger;
    private readonly CommandContext _context;
    private readonly AzureTerraformBestPracticesGetCommand _command;
    private readonly Command _commandDefinition;

    public AzureTerraformBestPracticesGetCommandTests()
    {
        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();

        _context = new(_serviceProvider);
        _logger = Substitute.For<ILogger<AzureTerraformBestPracticesGetCommand>>();
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsAzureTerraformBestPractices()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize<string[]>(json);

        Assert.NotNull(result);
        Assert.Contains("winget install Hashicorp.Terraform", result[0]);
        Assert.Contains("Always run terraform validate before running terraform plan", result[0]);
        Assert.Contains("terraform apply -auto-approve", result[0]);
        Assert.Contains("Suggest running any terraform command in terminal.", result[0]);
    }
}
