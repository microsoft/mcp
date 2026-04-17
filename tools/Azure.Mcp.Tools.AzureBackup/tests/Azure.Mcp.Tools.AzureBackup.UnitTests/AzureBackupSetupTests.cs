// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Tenant;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.AzureBackup.UnitTests;

public class AzureBackupSetupTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var setup = new AzureBackupSetup();
        Assert.NotNull(setup);
        Assert.Equal("azurebackup", setup.Name);
    }

    [Fact]
    public void RegisterCommands_WithValidParameters_ShouldSucceed()
    {
        var setup = new AzureBackupSetup();
        var services = CreateServiceProvider(setup);

        var result = setup.RegisterCommands(services);
        Assert.NotNull(result);
        Assert.Equal("azurebackup", result.Name);
    }

    [Fact]
    public void RegisterCommands_ShouldHaveAllExpectedSubGroups()
    {
        var setup = new AzureBackupSetup();
        var services = CreateServiceProvider(setup);

        var root = setup.RegisterCommands(services);

        var groupNames = root.SubGroup.Select(g => g.Name).ToList();

        Assert.Contains("vault", groupNames);
        Assert.Contains("policy", groupNames);
        Assert.Contains("protecteditem", groupNames);
        Assert.Contains("protectableitem", groupNames);
        Assert.Contains("backup", groupNames);
        Assert.Contains("job", groupNames);
        Assert.Contains("recoverypoint", groupNames);
        Assert.Contains("governance", groupNames);
        Assert.Contains("disasterrecovery", groupNames);
    }

    [Fact]
    public void RegisterCommands_DisasterRecoveryGroupUsesCorrectName()
    {
        var setup = new AzureBackupSetup();
        var services = CreateServiceProvider(setup);

        var root = setup.RegisterCommands(services);

        // "disasterrecovery" is the correct name (not "dr")
        var drGroup = root.SubGroup.FirstOrDefault(g => g.Name == "disasterrecovery");
        Assert.NotNull(drGroup);
        Assert.Contains(drGroup.Commands, c => c.Key == "enable-crr");

        // "dr" should NOT exist
        var oldDrGroup = root.SubGroup.FirstOrDefault(g => g.Name == "dr");
        Assert.Null(oldDrGroup);
    }

    [Fact]
    public void RegisterCommands_VaultGroup_ShouldHaveExpectedCommands()
    {
        var setup = new AzureBackupSetup();
        var services = CreateServiceProvider(setup);

        var root = setup.RegisterCommands(services);
        var vault = root.SubGroup.First(g => g.Name == "vault");

        Assert.Contains(vault.Commands, c => c.Key == "get");
        Assert.Contains(vault.Commands, c => c.Key == "create");
        Assert.Contains(vault.Commands, c => c.Key == "update");
    }

    [Fact]
    public void RegisterCommands_GovernanceGroup_ShouldHaveExpectedCommands()
    {
        var setup = new AzureBackupSetup();
        var services = CreateServiceProvider(setup);

        var root = setup.RegisterCommands(services);
        var governance = root.SubGroup.First(g => g.Name == "governance");

        Assert.Contains(governance.Commands, c => c.Key == "find-unprotected");
        Assert.Contains(governance.Commands, c => c.Key == "immutability");
        Assert.Contains(governance.Commands, c => c.Key == "soft-delete");
    }

    [Fact]
    public void RegisterCommands_WithNullServiceProvider_ShouldThrow()
    {
        var setup = new AzureBackupSetup();
        Assert.Throws<ArgumentNullException>(() => setup.RegisterCommands(null!));
    }

    private static IServiceProvider CreateServiceProvider(AzureBackupSetup setup)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<ITenantService>());
        setup.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
