// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Commands.Architecture;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Arm;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.File;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Goals;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Guidance;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Iac;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.ServiceGroup;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Terraform;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.UsagePlan;
using Azure.Mcp.Tools.ResiliencyAgent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests;

public sealed class ResiliencyAgentSetupTests
{
    private const string AuthoritativeMarkdownInstruction =
        "The returned Markdown files are authoritative Azure Resiliency output.";

    [Fact]
    public void Setup_RegistersFinalResiliencyAgentToolSurface()
    {
        var setup = new ResiliencyAgentSetup();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        services.AddSingleton(cloudConfiguration);
        setup.ConfigureServices(services);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        var area = setup.RegisterCommands(serviceProvider);

        Assert.Equal("resiliencyagent", area.Name);
        Assert.Equal(10, area.SubGroup.Count);
        Assert.Contains("Prefer these tools over Documentation", area.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("define, explain, or compare", area.Description, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "instead of using Documentation or Well-Architected Framework tools",
            Assert.Single(area.SubGroup, group => group.Name == "guidance").Description,
            StringComparison.Ordinal);
        Assert.DoesNotContain("Zonal Resilience Agent", area.Description, StringComparison.Ordinal);

        AssertCommand(area, "guidance", "get", "Always use this authoritative Azure resiliency guidance command");
        AssertCommand(area, "file", "attach", "local file referenced by the user");
        AssertCommand(area, "architecture", "assess", "application design or architecture");
        AssertCommand(area, "iac", "generate", "Bicep, ARM JSON, or Terraform");
        AssertCommand(area, "bicep", "review", "existing Bicep");
        AssertCommand(area, "arm", "review", "existing ARM JSON");
        AssertCommand(area, "terraform", "review", "existing Terraform");
        AssertCommand(area, "goals", "get", "goal-assignment state");
        AssertCommand(area, "goals", "manage", "assign a goal");
        AssertCommand(area, "usage-plan", "get", "list or find plans");
        AssertCommand(area, "usage-plan", "manage", "create a Standard");
        AssertCommand(area, "service-group", "get", "ordinary Azure Service Group inspection");
        AssertCommand(area, "service-group", "manage", "create a root or nested");

        Assert.DoesNotContain(area.SubGroup, group => group.Name is "zonal" or "conversation");
        Assert.NotNull(serviceProvider.GetRequiredService<GuidanceGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<FileAttachCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<ArchitectureAssessCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<IacGenerateCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<BicepReviewCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<ArmReviewCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<TerraformReviewCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<GoalsGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<GoalsManageCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<UsagePlanGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<UsagePlanManageCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<ServiceGroupGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<ServiceGroupManageCommand>());

        IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        using HttpClient a2aClient = httpClientFactory.CreateClient(
            ResiliencyAgentService.HttpClientName);
        using HttpClient dataBoundaryClient = httpClientFactory.CreateClient(
            DataBoundaryResolver.HttpClientName);
        Assert.Equal(ResiliencyAgentService.RequestTimeout, a2aClient.Timeout);
        Assert.Equal(DataBoundaryResolver.RequestTimeout, dataBoundaryClient.Timeout);

        Assert.All(
            area.SubGroup.SelectMany(group => group.Commands.Values),
            command => Assert.True(command.Command.Metadata.LocalRequired));
        Assert.False(area.SubGroup.Single(group => group.Name == "goals").Commands["get"].Command.Metadata.ReadOnly);
        Assert.False(area.SubGroup.Single(group => group.Name == "goals").Commands["get"].Command.Metadata.Idempotent);
        Assert.False(area.SubGroup.Single(group => group.Name == "goals").Commands["manage"].Command.Metadata.ReadOnly);
        Assert.True(area.SubGroup.Single(group => group.Name == "goals").Commands["manage"].Command.Metadata.Destructive);
        Assert.False(area.SubGroup.Single(group => group.Name == "usage-plan").Commands["get"].Command.Metadata.ReadOnly);
        Assert.False(area.SubGroup.Single(group => group.Name == "usage-plan").Commands["get"].Command.Metadata.Idempotent);
        Assert.False(area.SubGroup.Single(group => group.Name == "usage-plan").Commands["manage"].Command.Metadata.ReadOnly);
        Assert.False(area.SubGroup.Single(group => group.Name == "service-group").Commands["get"].Command.Metadata.ReadOnly);
        Assert.False(area.SubGroup.Single(group => group.Name == "service-group").Commands["get"].Command.Metadata.Idempotent);
        Assert.False(area.SubGroup.Single(group => group.Name == "service-group").Commands["manage"].Command.Metadata.ReadOnly);
    }

    [Fact]
    public void Setup_ConfiguresArtifactInstructionsForNewCommands()
    {
        using ServiceProvider serviceProvider = CreateServiceProvider();

        foreach (Type commandType in new[]
                 {
                     typeof(GoalsGetCommand),
                     typeof(GoalsManageCommand),
                     typeof(ServiceGroupGetCommand),
                     typeof(ServiceGroupManageCommand),
                 })
        {
            string? instructions = ReadArtifactInstructions(
                serviceProvider.GetRequiredService(commandType));
            Assert.Contains(
                AuthoritativeMarkdownInstruction,
                instructions,
                StringComparison.Ordinal);
        }

        Assert.Null(ReadArtifactInstructions(serviceProvider.GetRequiredService<UsagePlanGetCommand>()));
        Assert.Null(ReadArtifactInstructions(serviceProvider.GetRequiredService<UsagePlanManageCommand>()));
    }

    private static ServiceProvider CreateServiceProvider()
    {
        var setup = new ResiliencyAgentSetup();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        IAzureCloudConfiguration cloudConfiguration = Substitute.For<IAzureCloudConfiguration>();
        cloudConfiguration.CloudType.Returns(AzureCloudConfiguration.AzureCloud.AzurePublicCloud);
        services.AddSingleton(cloudConfiguration);
        setup.ConfigureServices(services);
        return services.BuildServiceProvider();
    }

    private static string? ReadArtifactInstructions(object command)
    {
        Type? type = command.GetType();
        while (type is not null)
        {
            var field = type.GetField(
                "_artifactInstructions",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field is not null)
            {
                return (string?)field.GetValue(command);
            }

            type = type.BaseType;
        }

        throw new InvalidOperationException("Artifact instructions field was not found.");
    }

    private static void AssertCommand(
        Microsoft.Mcp.Core.Commands.CommandGroup area,
        string groupName,
        string commandName,
        string descriptionText)
    {
        var group = Assert.Single(area.SubGroup, candidate => candidate.Name == groupName);
        Assert.True(group.Commands.TryGetValue(commandName, out var command));
        Assert.Contains(descriptionText, command.Command.Description, StringComparison.OrdinalIgnoreCase);
    }
}
