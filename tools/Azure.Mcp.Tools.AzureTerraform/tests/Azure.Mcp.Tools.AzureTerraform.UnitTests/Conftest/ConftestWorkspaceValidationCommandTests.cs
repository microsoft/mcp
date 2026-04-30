// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Commands;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Mcp.Tests.Client;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Azure.Mcp.Tools.AzureTerraform.UnitTests.Conftest;

public class ConftestWorkspaceValidationCommandTests : CommandUnitTestsBase<ConftestWorkspaceValidationCommand, IConftestService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("workspace", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Id);
        Assert.True(Command.Metadata.LocalRequired);
        Assert.True(Command.Metadata.ReadOnly);
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

        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateWorkspaceValidationCommand(
            workspaceFolder, "all", null, null)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--workspace-folder", workspaceFolder);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ConftestNotAvailable_ReturnsInstallationHelp()
    {
        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var response = await ExecuteCommandAsync("--workspace-folder", "/home/user/project");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingWorkspaceFolder_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync([]);

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

        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateWorkspaceValidationCommand(workspaceFolder, "avmsec", null, null)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync(
            "--workspace-folder", workspaceFolder,
            "--policy-set", "avmsec");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var response = await ExecuteCommandAsync("--workspace-folder", "/home/user/project");

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
            Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            Service.GenerateWorkspaceValidationCommand(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>())
                .Returns(new ConftestCommandResult { ConftestFound = true, Command = "conftest", Args = [], Description = "test" });
        }

        var response = await ExecuteCommandAsync(args);

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

        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GenerateWorkspaceValidationCommand(workspaceFolder, "all", null, null)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--workspace-folder", workspaceFolder);

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.ConftestCommandResult);

        Assert.True(result.ConftestFound);
        Assert.Equal("conftest", result.Command);
        Assert.NotNull(result.Args);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([
            "--workspace-folder", "/home/user/project",
            "--policy-set", "avmsec",
            "--severity-filter", "high"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--workspace-folder");
        Assert.Contains(options, o => o.Name == "--policy-set");
        Assert.Contains(options, o => o.Name == "--severity-filter");
    }
}
