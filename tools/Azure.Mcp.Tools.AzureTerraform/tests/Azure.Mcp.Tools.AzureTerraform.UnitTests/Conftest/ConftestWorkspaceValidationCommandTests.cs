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

public class ConftestWorkspaceValidationCommandTests
{
    private readonly ILogger<ConftestWorkspaceValidationCommand> _logger;
    private readonly IConftestService _conftestService;
    private readonly CommandContext _context;
    private readonly ConftestWorkspaceValidationCommand _command;
    private readonly Command _commandDefinition;

    public ConftestWorkspaceValidationCommandTests()
    {
        var collection = new ServiceCollection();
        var serviceProvider = collection.BuildServiceProvider();
        _context = new(serviceProvider);
        _logger = Substitute.For<ILogger<ConftestWorkspaceValidationCommand>>();
        _conftestService = Substitute.For<IConftestService>();
        _command = new(_logger, _conftestService);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("workspace", _command.Name);
        Assert.NotEmpty(_command.Description);
        Assert.NotEmpty(_command.Id);
        Assert.True(_command.Metadata.LocalRequired);
        Assert.True(_command.Metadata.ReadOnly);
    }

    [Fact]
    public async Task ExecuteAsync_ConftestAvailable_ReturnsCommand()
    {
        var workspaceFolder = "/home/user/terraform-project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy", "."],
            Description = $"Validate Terraform workspace: {workspaceFolder}"
        };

        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _conftestService.GenerateWorkspaceValidationCommand(
            workspaceFolder, "all", null, null)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--workspace-folder", workspaceFolder]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ConftestNotAvailable_ReturnsInstallationHelp()
    {
        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var args = _commandDefinition.Parse(["--workspace-folder", "/home/user/project"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingWorkspaceFolder_ReturnsValidationError()
    {
        var args = _commandDefinition.Parse([]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
    }

    [Fact]
    public async Task ExecuteAsync_WithPolicySet_PassesPolicySet()
    {
        var workspaceFolder = "/home/user/project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy/avmsec", "."],
            Description = $"Validate Terraform workspace: {workspaceFolder}",
            PolicySet = "avmsec"
        };

        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _conftestService.GenerateWorkspaceValidationCommand(
            workspaceFolder, "avmsec", null, null)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--workspace-folder", workspaceFolder, "--policy-set", "avmsec"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var args = _commandDefinition.Parse(["--workspace-folder", "/home/user/project"]);
        var response = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        Assert.NotEqual(HttpStatusCode.OK, response.Status);
    }

    [Theory]
    [InlineData("--workspace-folder /home/user/project", true)]
    [InlineData("--workspace-folder /home/user/project --policy-set avmsec --severity-filter high", true)]
    [InlineData("", false)]
    public async Task ExecuteAsync_ValidatesInputCorrectly(string args, bool shouldSucceed)
    {
        if (shouldSucceed)
        {
            _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            _conftestService.GenerateWorkspaceValidationCommand(
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
        var workspaceFolder = "/home/user/terraform-project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy", "."],
            Description = $"Validate Terraform workspace: {workspaceFolder}",
            PolicySet = "all"
        };

        _conftestService.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        _conftestService.GenerateWorkspaceValidationCommand(
            workspaceFolder, "all", null, null)
            .Returns(expectedResult);

        var args = _commandDefinition.Parse(["--workspace-folder", workspaceFolder]);
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
            "--workspace-folder", "/home/user/project",
            "--policy-set", "avmsec",
            "--severity-filter", "high"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var command = _command.GetCommand();
        var options = command.Options;

        Assert.Contains(options, o => o.Name == "--workspace-folder");
        Assert.Contains(options, o => o.Name == "--policy-set");
        Assert.Contains(options, o => o.Name == "--severity-filter");
    }
}
