// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Deploy.Services.Util;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests;

public sealed class DeploymentPlanTemplateUtilV2Tests
{
    [Theory]
    [InlineData("", "WebApp")]
    [InlineData("TestProject", "ContainerApp")]
    public void GetPlanTemplate_ValidInputs_ReturnsFormattedTemplate(
        string projectName,
        string targetAppService)
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            projectName,
            targetAppService
        );

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

        // Should contain Azure CLI content
        Assert.Contains("Azure CLI", result);
    }

    [Fact]
    public void GetPlanTemplate_EmptyProjectName_UsesDefaultTitle()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "",
            "ContainerApp");

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
            "ContainerApp");

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
            targetAppService);

        // Assert
        Assert.Contains(expectedAzureHost, result);
    }

    [Fact]
    public void GetPlanTemplate_AksTarget_IncludesKubernetesSteps()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "TestProject",
            "AKS");

        // Assert
        Assert.Contains("kubectl apply", result);
        Assert.Contains("Kubernetes", result);
        Assert.Contains("pods are running", result);
    }

    [Fact]
    public void GetPlanTemplate_ContainerApp_IncludesDockerSteps()
    {
        // Act
        var result = DeploymentPlanTemplateUtil.GetPlanTemplate(
            "TestProject",
            "ContainerApp");

        // Assert
        Assert.Contains("Build and Push Docker Image", result);
        Assert.Contains("Dockerfile", result);
    }
}
