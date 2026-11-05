// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Configuration;
using Microsoft.Mcp.Core.Services.Telemetry;
using NSubstitute;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class CommandRegistrationTests
{
    [Theory]
    [InlineData("storage")]
    [InlineData("subscription")]
    [InlineData("Storage")]
    public void Factory_BindsSetupIdentityAcrossNestedGroups(string namespaceName)
    {
        IBaseCommand command = CreateCommand();
        var group = new CommandGroup("display", "Displayed group");
        var nested = new CommandGroup("resource", "Resource commands");
        nested.AddCommand(command);
        group.AddSubGroup(nested);
        CommandFactory factory = CreateFactory(namespaceName, group);

        CommandRegistration registration = Assert.IsType<CommandRegistration>(
            factory.FindCommandRegistration("display_resource_get"));
        Assert.Equal(namespaceName, registration.ToolNamespaceName);
        Assert.Same(command, registration.Command);
        Assert.Same(command, factory.FindCommandByName("display_resource_get"));
        Assert.Same(registration, nested.Commands["get"]);
        Assert.Same(registration, factory.AllCommands["display_resource_get"]);
        Assert.Same(registration, factory.GroupCommands(["DISPLAY"])["display_resource_get"]);
        Assert.False(registration.NeedsNamespaceBinding);
        Assert.Null(factory.FindCommandRegistration("unknown"));
    }

    [Fact]
    public void Factory_PreservesSameRegistrationAcrossRepeatedRegrouping()
    {
        IBaseCommand command = CreateCommand();
        var originalGroup = new CommandGroup("storage", "Original group");
        originalGroup.AddCommand(command);
        CommandRegistration original = CreateFactory("storage", originalGroup).AllCommands["storage_get"];

        var consolidatedGroup = new CommandGroup("synthetic", "Consolidated group");
        consolidatedGroup.AddCommand("storage_get", original);
        CommandFactory consolidated = CreateFactory("synthetic", consolidatedGroup);
        CommandRegistration regrouped = consolidated.AllCommands["synthetic_storage_get"];
        Assert.Same(original, regrouped);
        Assert.Equal("storage", regrouped.ToolNamespaceName);
        Assert.Equal("synthetic", consolidated.GetServiceArea("synthetic_storage_get"));

        var nextGroup = new CommandGroup("proxy", "Another presentation");
        nextGroup.AddCommand("alias", regrouped);
        CommandFactory next = CreateFactory("proxy", nextGroup);
        Assert.Same(original, next.FindCommandRegistration("proxy_alias"));
        Assert.Equal("storage", next.AllCommands["proxy_alias"].ToolNamespaceName);
    }

    [Fact]
    public void Factory_UnresolvedRegistrationWarnsWithoutInventingSyntheticNamespace()
    {
        var unresolved = new CommandRegistration(CreateCommand(), null);
        var group = new CommandGroup("synthetic", "Displayed group");
        group.AddCommand("get", unresolved);
        ILogger<CommandFactory> logger = Substitute.For<ILogger<CommandFactory>>();
        CommandFactory factory = CreateFactory("synthetic", group, logger);

        Assert.Same(unresolved, factory.FindCommandRegistration("synthetic_get"));
        Assert.Null(factory.AllCommands["synthetic_get"].ToolNamespaceName);
        Assert.Contains(logger.ReceivedCalls(), call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log) &&
            Equals(call.GetArguments()[0], LogLevel.Warning));
    }

    [Fact]
    public void Factory_SelectsRegistrationIdentityEvenWhenCommandInstanceIsShared()
    {
        IBaseCommand command = CreateCommand();
        var storage = new CommandRegistration(command, "storage");
        var compute = new CommandRegistration(command, "compute");
        var group = new CommandGroup("synthetic", "Displayed group");
        var storageGroup = new CommandGroup("first", "First route");
        var computeGroup = new CommandGroup("second", "Second route");
        storageGroup.AddCommand("get", storage);
        computeGroup.AddCommand("get", compute);
        group.AddSubGroup(storageGroup);
        group.AddSubGroup(computeGroup);
        CommandFactory factory = CreateFactory("synthetic", group);

        Assert.Same(storage, factory.FindCommandRegistration("synthetic_first_get"));
        Assert.Same(compute, factory.FindCommandRegistration("synthetic_second_get"));
        Assert.Same(command, factory.FindCommandByName("synthetic_first_get"));
        Assert.Same(command, factory.FindCommandByName("synthetic_second_get"));
        Assert.Equal("storage", factory.AllCommands["synthetic_first_get"].ToolNamespaceName);
        Assert.Equal("compute", factory.AllCommands["synthetic_second_get"].ToolNamespaceName);
    }

    [Fact]
    public void Registration_AddsExistingRegistrationThroughNestedPath()
    {
        var registration = new CommandRegistration(CreateCommand(), "storage");
        var group = new CommandGroup("synthetic", "Displayed group");
        var nested = new CommandGroup("resource", "Resource commands");
        group.AddSubGroup(nested);

        group.AddCommand("resource.get", registration);
        Assert.Same(registration, nested.Commands["get"]);
        Assert.Same(registration.Command, group.GetCommand("resource.get"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Registration_UnusableSetupNameRemainsUnresolved(string namespaceName)
    {
        var group = new CommandGroup("display", "Displayed group");
        group.AddCommand(CreateCommand());
        CommandRegistration registration = group.Commands["get"].BindNamespace(namespaceName);

        Assert.Null(registration.ToolNamespaceName);
        Assert.False(registration.NeedsNamespaceBinding);
        Assert.Same(registration, registration.BindNamespace("synthetic"));
    }

    [Fact]
    public void Registration_BindsOnlyNewSetupDeclarations()
    {
        IBaseCommand command = CreateCommand();
        var group = new CommandGroup("storage", "Storage commands");
        group.AddCommand(command);
        CommandRegistration pending = group.Commands["get"];
        Assert.True(pending.NeedsNamespaceBinding);

        CommandRegistration original = pending.BindNamespace("storage");
        Assert.False(original.NeedsNamespaceBinding);
        Assert.Equal("storage", original.ToolNamespaceName);
        Assert.Same(original, original.BindNamespace("synthetic"));
        Assert.Same(command, group.GetCommand("get"));
        Assert.True(group.AllToolsInGroupMatch(metadata => metadata.ReadOnly));
    }

    private static IBaseCommand CreateCommand()
    {
        IBaseCommand command = Substitute.For<IBaseCommand>();
        command.Name.Returns("get");
        command.Id.Returns("879b918f-9d94-4941-9c9f-6158262d5c99");
        command.GetCommand().Returns(new Command("get", "Gets a resource"));
        command.Metadata.Returns(new ToolMetadata { ReadOnly = true });
        return command;
    }

    private static CommandFactory CreateFactory(
        string namespaceName,
        CommandGroup group,
        ILogger<CommandFactory>? logger = null)
    {
        IAreaSetup area = Substitute.For<IAreaSetup>();
        area.Name.Returns(namespaceName);
        area.RegisterCommands(Arg.Any<IServiceProvider>()).Returns(group);
        return new CommandFactory(
            Substitute.For<IServiceProvider>(),
            [area],
            Substitute.For<ITelemetryService>(),
            Microsoft.Extensions.Options.Options.Create(new McpServerConfiguration
            {
                Name = "RegistrationTest",
                ShortName = "test",
                Version = "1.0.0",
                RootCommandGroupName = "test",
                DisplayName = "Command registration test",
                Description = "Test command registrations"
            }),
            logger ?? Substitute.For<ILogger<CommandFactory>>());
    }
}
