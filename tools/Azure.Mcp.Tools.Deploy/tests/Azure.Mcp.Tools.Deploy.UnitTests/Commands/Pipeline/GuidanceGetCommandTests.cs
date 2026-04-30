// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.Deploy.Commands.Pipeline;
using Microsoft.Mcp.Tests.Client;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Commands.Pipeline;


public class GuidanceGetCommandTests : CommandUnitTestsBase<GuidanceGetCommand, object>
{
    [Fact]
    public async Task Should_generate_pipeline()
    {
        // arrange & act
        var result = await ExecuteCommandAsync("--subscription", "test-subscription-id");

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
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--is-azd-project", "false",
            "--pipeline-platform", "github-actions",
            "--deploy-option", "deploy-only");

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
        // arrange & act - not providing is-azd-project should default to false
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--is-azd-project", "false",
            "--pipeline-platform", "azure-devops",
            "--deploy-option", "deploy-only");

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
        // arrange & act
        var result = await ExecuteCommandAsync(
            "--subscription", "test-subscription-id",
            "--is-azd-project", "false",
            "--deploy-option", "provision-and-deploy");

        // assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Message);
        Assert.Contains("When user wants to include provisioning", result.Message);
    }
}
