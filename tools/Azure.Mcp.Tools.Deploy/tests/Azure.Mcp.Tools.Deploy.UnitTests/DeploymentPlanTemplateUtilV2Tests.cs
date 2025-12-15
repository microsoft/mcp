// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Services.Util;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests;

public sealed class DeploymentPlanTemplateUtilV2Tests
{
    [Theory]
    [InlineData("TestProject", "ContainerApp", "AZD", "bicep")]
    [InlineData("", "WebApp", "AzCli", "")]
    [InlineData("MyApp", "AKS", "AZD", "terraform")]
    public void GetPlanTemplate_ValidInputs_ReturnsFormattedTemplate(
        string projectName,
        string targetAppService,
        string provisioningTool,
        string azdIacOptions)
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            projectName,
            targetAppService,
            provisioningTool,
            azdIacOptions);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        // Should contain expected sections
        Assert.Contains("## **Goal**", result);
        Assert.Contains("## **Project Information**", result);
        Assert.Contains("## **Azure Resources Architecture**", result);
        Assert.Contains("## **Recommended Azure Resources**", result);
        Assert.Contains("## **Execution Step**", result);

        // Should not contain unprocessed placeholders for main content
        Assert.DoesNotContain("{{Title}}", result);
        Assert.DoesNotContain("{{ProvisioningTool}}", result);

        // Should contain appropriate provisioning tool
        if (provisioningTool.ToLowerInvariant() == "azd")
        {
            Assert.Contains("mcp_azure_mcp_azd", result);
        }
        else
        {
            Assert.Contains("Azure CLI", result);
        }
    }

    [Fact]
    public void GetPlanTemplate_EmptyProjectName_UsesDefaultTitle()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "",
            "ContainerApp",
            "AZD",
            "bicep");

        // Assert
        Assert.Contains("Azure Deployment Plan", result);
        Assert.DoesNotContain("Azure Deployment Plan for  Project", result);
    }

    [Fact]
    public void GetPlanTemplate_WithProjectName_UsesProjectSpecificTitle()
    {
        // Arrange
        var projectName = "MyTestProject";

        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            projectName,
            "ContainerApp",
            "AZD",
            "bicep");

        // Assert
        Assert.Contains($"Azure Deployment Plan for {projectName} Project", result);
    }

    [Theory]
    [InlineData("containerapp", "Azure Container Apps")]
    [InlineData("webapp", "Azure Web App Service")]
    [InlineData("functionapp", "Azure Functions")]
    [InlineData("aks", "Azure Kubernetes Service")]
    [InlineData("unknown", "Azure Container Apps")] // Default case
    public void GetPlanTemplate_DifferentTargetServices_MapsToCorrectAzureHost(
        string targetAppService,
        string expectedAzureHost)
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "TestProject",
            targetAppService,
            "AZD",
            "bicep");

        // Assert
        Assert.Contains(expectedAzureHost, result);
    }

    [Fact]
    public void GetPlanTemplate_AzdWithoutIacOptions_DefaultsToBicep()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "TestProject",
            "ContainerApp",
            "azd",
            "");

        // Assert
        Assert.Contains("bicep", result);
    }

    [Fact]
    public void GetPlanTemplate_AksTarget_IncludesKubernetesSteps()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "TestProject",
            "AKS",
            "AZD",
            "bicep");

        // Assert
        Assert.Contains("kubectl apply", result);
        Assert.Contains("Kubernetes", result);
        Assert.Contains("pods are running", result);
    }

    [Fact]
    public void GetPlanTemplate_ContainerAppWithAzCli_IncludesDockerSteps()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "TestProject",
            "ContainerApp",
            "AzCli",
            "");

        // Assert
        Assert.Contains("Build and Push Docker Image", result);
        Assert.Contains("Dockerfile", result);
    }
}
