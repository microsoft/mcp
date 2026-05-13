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

public class ConftestPlanValidationCommandTests : CommandUnitTestsBase<ConftestPlanValidationCommand, IConftestService>
{
    [Fact]
    public void Constructor_InitializesCommandCorrectly()
    {
        Assert.Equal("plan", Command.Name);
        Assert.NotEmpty(Command.Description);
        Assert.NotEmpty(Command.Id);
        Assert.True(Command.Metadata.LocalRequired);
        Assert.True(Command.Metadata.ReadOnly);
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

        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GeneratePlanValidationCommand(
            planFolder, "all", null, null)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--plan-folder", planFolder);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ConftestNotAvailable_ReturnsInstallationHelp()
    {
        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(false);

        var response = await ExecuteCommandAsync("--plan-folder", "/home/user/project");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_MissingPlanFolder_ReturnsValidationError()
    {
        var response = await ExecuteCommandAsync([]);

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

        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GeneratePlanValidationCommand(
            planFolder, "avmsec", "high", null)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync(
            "--plan-folder", planFolder,
            "--policy-set", "avmsec",
            "--severity-filter", "high");

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.NotNull(response.Results);
    }

    [Fact]
    public async Task ExecuteAsync_ServiceThrows_HandlesException()
    {
        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var response = await ExecuteCommandAsync("--plan-folder", "/home/user/project");

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
            Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            Service.GeneratePlanValidationCommand(
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
        var planFolder = "/home/user/terraform-project";
        var expectedResult = new ConftestCommandResult
        {
            ConftestFound = true,
            Command = "conftest",
            Args = ["test", "--all-namespaces", "--output", "json", "-p", "./policy", "tfplan.json"],
            Description = $"Validate Terraform plan in: {planFolder}",
            PolicySet = "all"
        };

        Service.IsConftestAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        Service.GeneratePlanValidationCommand(
            planFolder, "all", null, null)
            .Returns(expectedResult);

        var response = await ExecuteCommandAsync("--plan-folder", planFolder);

        var result = ValidateAndDeserializeResponse(response, AzureTerraformJsonContext.Default.ConftestCommandResult);

        Assert.True(result.ConftestFound);
        Assert.Equal("conftest", result.Command);
        Assert.NotNull(result.Args);
    }

    [Fact]
    public void BindOptions_BindsOptionsCorrectly()
    {
        var args = CommandDefinition.Parse([
            "--plan-folder", "/home/user/project",
            "--policy-set", "avmsec",
            "--severity-filter", "high"
        ]);

        Assert.NotNull(args);
        Assert.Empty(args.Errors);

        var options = CommandDefinition.Options;

        Assert.Contains(options, o => o.Name == "--plan-folder");
        Assert.Contains(options, o => o.Name == "--policy-set");
        Assert.Contains(options, o => o.Name == "--severity-filter");
    }
}
