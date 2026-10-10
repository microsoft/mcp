// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ResiliencyAgent.Commands.Architecture;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Arm;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Bicep;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.File;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Guidance;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Iac;
using Azure.Mcp.Tools.ResiliencyAgent.Commands.Terraform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Services.Azure.Authentication;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.ResiliencyAgent.Tests;

public sealed class ResiliencyAgentSetupTests
{
    [Fact]
    public void Setup_RegistersFinalResiliencyAgentToolSurface()
    {
        var setup = new ResiliencyAgentSetup();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IAzureTokenCredentialProvider>());
        setup.ConfigureServices(services);
        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        var area = setup.RegisterCommands(serviceProvider);

        Assert.Equal("resiliencyagent", area.Name);
        Assert.Equal(7, area.SubGroup.Count);
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

        Assert.DoesNotContain(area.SubGroup, group => group.Name is "zonal" or "conversation");
        Assert.NotNull(serviceProvider.GetRequiredService<GuidanceGetCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<FileAttachCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<ArchitectureAssessCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<IacGenerateCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<BicepReviewCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<ArmReviewCommand>());
        Assert.NotNull(serviceProvider.GetRequiredService<TerraformReviewCommand>());

        Assert.All(
            area.SubGroup.SelectMany(group => group.Commands.Values),
            command =>
            {
                Assert.True(command.Metadata.LocalRequired);
                Assert.False(command.Metadata.ReadOnly);
            });
    }

    private static void AssertCommand(
        Microsoft.Mcp.Core.Commands.CommandGroup area,
        string groupName,
        string commandName,
        string descriptionText)
    {
        var group = Assert.Single(area.SubGroup, candidate => candidate.Name == groupName);
        var command = Assert.Single(group.Commands);
        Assert.Equal(commandName, command.Key);
        Assert.Contains(descriptionText, command.Value.Description, StringComparison.OrdinalIgnoreCase);
    }
}
