// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using System.Text.Json;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests;

public class ConftestPlanValidationCommandTests
{
    private readonly ILogger<ConftestPlanValidationCommand> _logger;
    private readonly IConftestService _conftestService;
    private readonly CommandContext _context;
    private readonly ConftestPlanValidationCommand _command;
    private readonly Command _commandDefinition;

    public ConftestPlanValidationCommandTests()
    {
        var collection = new ServiceCollection();
        var serviceProvider = collection.BuildServiceProvider();
        _context = new(serviceProvider);
        _logger = Substitute.For<ILogger<ConftestPlanValidationCommand>>();
        _conftestService = Substitute.For<IConftestService>();
        _command = new(_logger, _conftestService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("plan", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Id);
        Assert.True(_command.Metadata.LocalRequired);
        Assert.True(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ConftestAvailable_ReturnsCommand()
    {
        var planFolder = "/home/user/terraform-project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy", "tfplan.json"],
            Description = $"Validate Terraform plan in: {planFolder}"
        };

        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _conftestService.GeneratePlanValidationCommand(
            planFolder, "all", null, null)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--plan-folder", planFolder]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ConftestNotAvailable_ReturnsInstallationHelp()
    {
        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var args = _commandDefinition.Parse(["--plan-folder", "/home/user/project"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingPlanFolder_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithPolicySetAndSeverity_PassesOptions()
    {
        var planFolder = "/home/user/project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy/avmsec", "-p", ".conftest_severity_high.rego", "tfplan.json"],
            Description = $"Validate Terraform plan in: {planFolder}",
            PolicySet = "avmsec"
        };

        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _conftestService.GeneratePlanValidationCommand(
            planFolder, "avmsec", "high", null)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--plan-folder", planFolder, "--policy-set", "avmsec", "--severity-filter", "high"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var args = _commandDefinition.Parse(["--plan-folder", "/home/user/project"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--plan-folder /home/user/project", true)]
    [InlineData("--plan-folder /home/user/project --policy-set avmsec --severity-filter high", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            _conftestService.GeneratePlanValidationCommand(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>())
                .Returns(new ConftestCommandResult { ConftestFound = true, Command = "conftest", Args = [], Description = "test" });
        }

        var parseResult = _commandDefinition.Parse(args);
        var response = await _command.ExecuteAsync(_context, parseResult, TestContext.Current.CancellationToken);

        if (shouldSucceed)
        {
            Assert.Equal(HttpStatusCode.OK, response.Status);
        }
        else
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        }
    }

    [Fact]
    public async Task ExecuteAsync_DeserializationValidation()
    {
        var planFolder = "/home/user/terraform-project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy", "tfplan.json"],
            Description = $"Validate Terraform plan in: {planFolder}",
            PolicySet = "all"
        };

        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _conftestService.GeneratePlanValidationCommand(
            planFolder, "all", null, null)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--plan-folder", planFolder]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);

        var json = JsonSerializer.Serialize(response.Results);
        var result = JsonSerializer.Deserialize(json, AzureTerraformJsonContext.Default.ConftestCommandResult);

        Assert.NotNull(result);
        Assert.True(result.ConftestFound);
        Assert.Equal("conftest", result.Command);
        Assert.NotNull(result.Args);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = _commandDefinition.Parse([
            "--plan-folder", "/home/user/project",
            "--policy-set", "avmsec",
            "--severity-filter", "high"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var command = _command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--plan-folder");
        Assert.Contains(options, o => o.Name == "--policy-set");
        Assert.Contains(options, o => o.Name == "--severity-filter");
    }
}
