// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Core.Services.Azure.Tenant;
using Azure.Mcp.Tools.Compute.Commands;
using Azure.Mcp.Tools.Compute.Commands.Vm;
using Azure.Mcp.Tools.Compute.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Tools.Compute.Tests;

/// <summary>
/// MCP surface smoke tests for the Compute area: verifies that the unified
/// <c>compute create</c> command and the four top-level discovery aliases
/// (<c>list-skus</c>, <c>list-images</c>, <c>check-quota</c>, <c>recommend-region</c>)
/// are reachable directly under the <c>compute</c> root group, that the
/// back-compat <c>vm</c>/<c>vmss</c>/<c>disk</c> subgroups still resolve, and that
/// the root description still leads with the VMSS Flex recommendation.
/// </summary>
public class ComputeSetupTests
{
    [Fact]
    public void Constructor_ShouldCreateInstance()
    {
        var setup = new ComputeSetup();
        Assert.NotNull(setup);
        Assert.Equal("compute", setup.Name);
        Assert.False(string.IsNullOrWhiteSpace(setup.Title));
    }

    [Fact]
    public void RegisterCommands_WithValidParameters_ShouldSucceed()
    {
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var result = setup.RegisterCommands(services);

        Assert.NotNull(result);
    }

    [Fact]
    public void RegisterCommands_ShouldReturnComputeGroup()
    {
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        Assert.NotNull(computeGroup);
        Assert.Equal("compute", computeGroup.Name);
        Assert.False(string.IsNullOrWhiteSpace(computeGroup.Description));
    }

    [Fact]
    public void RegisterCommands_ComputeGroup_DescriptionShouldRecommendVmssFlex()
    {
        // The reframe is load-bearing: agents should pick up the VMSS Flex default
        // straight from the group description, with single-VM positioned as a fallback.
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        Assert.NotNull(computeGroup);
        Assert.Contains("VMSS Flex", computeGroup.Description);
        Assert.Contains("recommended default", computeGroup.Description);
        Assert.Contains("compute create", computeGroup.Description);
        Assert.Contains("fallback", computeGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddVmSubGroup()
    {
        // Back-compat: existing `compute vm …` callers must keep resolving.
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        var vmGroup = computeGroup.SubGroup.FirstOrDefault(g => g.Name == "vm");
        Assert.NotNull(vmGroup);
        Assert.Contains("Virtual Machine operations", vmGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddVmssSubGroup()
    {
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        var vmssGroup = computeGroup.SubGroup.FirstOrDefault(g => g.Name == "vmss");
        Assert.NotNull(vmssGroup);
        Assert.Contains("Virtual Machine Scale Set", vmssGroup.Description);
    }

    [Fact]
    public void RegisterCommands_ShouldAddDiskSubGroup()
    {
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        var diskGroup = computeGroup.SubGroup.FirstOrDefault(g => g.Name == "disk");
        Assert.NotNull(diskGroup);
        Assert.Contains("Managed Disk", diskGroup.Description);
    }

    [Fact]
    public void RegisterCommands_VmSubGroup_ShouldRegisterCreateAndDiscoveryCommands()
    {
        // The original VM-scoped wiring stays intact — discovery commands are still
        // reachable at `compute vm list-skus` etc. for callers using the older path.
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);
        var vmGroup = computeGroup.SubGroup.First(g => g.Name == "vm");

        Assert.Contains("create", vmGroup.Commands.Keys);
        Assert.Contains("list-skus", vmGroup.Commands.Keys);
        Assert.Contains("list-images", vmGroup.Commands.Keys);
        Assert.Contains("check-quota", vmGroup.Commands.Keys);
        Assert.Contains("recommend-region", vmGroup.Commands.Keys);
    }

    [Fact]
    public void RegisterCommands_VmssSubGroup_ShouldRegisterCreateCommand()
    {
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);
        var vmssGroup = computeGroup.SubGroup.First(g => g.Name == "vmss");

        Assert.Contains("create", vmssGroup.Commands.Keys);
    }

    [Fact]
    public void RegisterCommands_ShouldRegisterUnifiedCreateAtTopLevel()
    {
        // The recommended entry point: `compute create` (no subgroup).
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        Assert.Contains("create", computeGroup.Commands.Keys);
        var create = computeGroup.Commands["create"];
        Assert.IsType<UnifiedCreateCommand>(create);
    }

    [Fact]
    public void RegisterCommands_ShouldRegisterDiscoveryAliasesAtTopLevel()
    {
        // Step-4 validation gates: discoverable without descending into `vm/`.
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);

        Assert.Contains("list-skus", computeGroup.Commands.Keys);
        Assert.Contains("list-images", computeGroup.Commands.Keys);
        Assert.Contains("check-quota", computeGroup.Commands.Keys);
        Assert.Contains("recommend-region", computeGroup.Commands.Keys);

        Assert.IsType<VmSkuListCommand>(computeGroup.Commands["list-skus"]);
        Assert.IsType<VmImageListCommand>(computeGroup.Commands["list-images"]);
        Assert.IsType<VmQuotaCheckCommand>(computeGroup.Commands["check-quota"]);
        Assert.IsType<VmRegionRecommendCommand>(computeGroup.Commands["recommend-region"]);
    }

    [Fact]
    public void RegisterCommands_DiscoveryAliases_ShouldShareInstancesWithVmSubGroup()
    {
        // The aliases must be the SAME class instances as the VM-subgroup registrations,
        // not parallel copies — that keeps the CommandMetadata.Id GUIDs stable and avoids
        // surfacing two distinct tools to MCP clients.
        var setup = new ComputeSetup();
        var services = CreateServiceProvider(setup);

        var computeGroup = setup.RegisterCommands(services);
        var vmGroup = computeGroup.SubGroup.First(g => g.Name == "vm");

        Assert.Same(vmGroup.Commands["list-skus"], computeGroup.Commands["list-skus"]);
        Assert.Same(vmGroup.Commands["list-images"], computeGroup.Commands["list-images"]);
        Assert.Same(vmGroup.Commands["check-quota"], computeGroup.Commands["check-quota"]);
        Assert.Same(vmGroup.Commands["recommend-region"], computeGroup.Commands["recommend-region"]);
    }

    [Fact]
    public void RegisterCommands_WithNullServiceProvider_ShouldThrow()
    {
        var setup = new ComputeSetup();

        Assert.Throws<ArgumentNullException>(() => setup.RegisterCommands(null!));
    }

    [Fact]
    public void ComputeSetup_TypeValidation_ShouldHaveCorrectProperties()
    {
        var type = typeof(ComputeSetup);

        Assert.True(type.IsClass);
        Assert.False(type.IsAbstract);
        Assert.False(type.IsInterface);

        var registerMethod = type.GetMethod("RegisterCommands", new[] { typeof(IServiceProvider) });
        Assert.NotNull(registerMethod);
        Assert.True(registerMethod.IsPublic);
        Assert.Equal(typeof(CommandGroup), registerMethod.ReturnType);
    }

    private static IServiceProvider CreateServiceProvider(ComputeSetup setup)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        // Compute commands depend on IComputeService (registered by setup) plus the
        // ambient subscription/tenant services used by BaseAzureCommand.
        services.AddSingleton(Substitute.For<ITenantService>());
        services.AddSingleton(Substitute.For<ISubscriptionService>());
        setup.ConfigureServices(services);
        return services.BuildServiceProvider();
    }
}
