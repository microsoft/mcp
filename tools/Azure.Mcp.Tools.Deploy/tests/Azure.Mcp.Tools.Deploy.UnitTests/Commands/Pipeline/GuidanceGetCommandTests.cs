// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Net;
using Azure.Mcp.Tools.Deploy.Commands.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Models.Command;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Pipeline;


public class GuidanceGetCommandTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GuidanceGetCommand> _logger;
    private readonly Command _commandDefinition;
    private readonly CommandContext _context;
    private readonly GuidanceGetCommand _command;

    public GuidanceGetCommandTests()
    {
        _logger = Substitute.For<ILogger<GuidanceGetCommand>>();

        var collection = new ServiceCollection();
        _serviceProvider = collection.BuildServiceProvider();
        _context = new(_serviceProvider);
        _command = new(_logger);
        _commandDefinition = _command.GetCommand();
    }

    [Fact]
    public async Task Should_generate_pipeline()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("When user confirms that Azure resources are ready for deployment", result.Message);
        Assert.Contains("Create a setup-azure-auth-for-pipeline.sh or setup-azure-auth-for-pipeline.ps1 script to automate the auth configuration.", result.Message);
        Assert.Contains("Create Github environments and set up approval checks in ALL environments.", result.Message);
        Assert.Contains("Use User-assigned Managed Identity with OIDC for login to Azure in the pipeline.", result.Message);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_github_actions()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--is-azd-project", "false",
            "--pipeline-platform", "github-actions",
            "--deploy-option", "deploy-only",
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("When user confirms that Azure resources are ready for deployment", result.Message);
        Assert.Contains("Create a setup-azure-auth-for-pipeline.sh or setup-azure-auth-for-pipeline.ps1 script to automate the auth configuration.", result.Message);
        Assert.Contains("Create Github environments and set up approval checks in ALL environments.", result.Message);
        Assert.Contains("Use User-assigned Managed Identity with OIDC for login to Azure in the pipeline.", result.Message);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_azure_devops()
    {
        // arrange - not providing is-azd-project should default to false
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--is-azd-project", "false",
            "--pipeline-platform", "azure-devops",
            "--deploy-option", "deploy-only",
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("When user confirms that Azure resources are ready for deployment", result.Message);
        Assert.Contains("You should use a .azure/pipeline-setup.md file to outline the steps.", result.Message);
        Assert.Contains("Use Service Principal(app registration) with workflow identity federation to login to Azure in the pipeline.", result.Message);
        Assert.Contains("Set up Service Connection in Azure DevOps using app registration with workflow identity federation.", result.Message);
    }

    [Fact]
    public async Task Should_generate_pipeline_with_provision_and_deploy()
    {
        // arrange
        var args = _commandDefinition.Parse([
            "--subscription", "test-subscription-id",
            "--is-azd-project", "false"
        ]);

        // act
        var result = await _command.ExecuteAsync(_context, args, TestContext.Current.CancellationToken);

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("When user wants to include provisioning", result.Message);
    }
}
