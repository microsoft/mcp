// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Areas.Server.Commands.ToolLoading;
using Microsoft.Mcp.Core.Commands;
using NSubstitute;
using Xunit;

namespace Azure.Mcp.Core.Tests.Areas.Server.Commands.ToolLoading;

public sealed class NamespaceCommandCatalogTests
{
    [Theory]
    [InlineData("storage_account_get", "account_get")]
    [InlineData("STORAGE_account_get", "account_get")]
    [InlineData("storage_get", "get")]
    [InlineData("storage_storage_get", "storage_get")]
    [InlineData("other_storage_get", "other_storage_get")]
    public void Create_RemovesOnlyLeadingNamespace(string registeredName, string expectedName)
    {
        var registration = new CommandRegistration(CreateCommand(), "original");
        var catalog = NamespaceCommandCatalog.Create("storage",
            new Dictionary<string, CommandRegistration> { [registeredName] = registration }, true);

        var command = Assert.Single(catalog.Commands).Value;
        Assert.Equal(expectedName, command.Name);
        Assert.Same(registration, command.Registration);
        Assert.Same(command, catalog.Resolve(expectedName.ToUpperInvariant()));
        Assert.Same(command, catalog.Resolve(registeredName));
    }

    [Fact]
    public void Create_OperationOverrideRetainsBothLegacySpellings()
    {
        var command = Substitute.For<IBaseCommand>();
        command.Name.Returns("get");
        command.NamespaceOperationName.Returns("list");
        var registration = new CommandRegistration(command, "original");
        var catalog = NamespaceCommandCatalog.Create("storage",
            new Dictionary<string, CommandRegistration> { ["storage_item_get"] = registration }, true);

        var route = Assert.Single(catalog.Commands).Value;
        Assert.Equal("item_list", route.Name);
        Assert.Same(route, catalog.Resolve("item_list"));
        Assert.Same(route, catalog.Resolve("item_get"));
        Assert.Same(route, catalog.Resolve("storage_item_get"));
        Assert.Null(catalog.Resolve("storage_item_list"));
        Assert.Null(catalog.Resolve("item list"));
        Assert.Null(catalog.Resolve("item-list"));
        Assert.Null(catalog.Resolve("other_item_get"));
        Assert.Same(registration, route.Registration);
    }

    [Fact]
    public void Create_WithoutOptInPreservesRegisteredNamesAndIgnoresOverrides()
    {
        var command = Substitute.For<IBaseCommand>();
        command.Name.Returns("get");
        command.NamespaceOperationName.Returns("list");
        var catalog = NamespaceCommandCatalog.Create("storage",
            new Dictionary<string, CommandRegistration> { ["storage_item_get"] = new(command, "storage") }, false);

        Assert.Equal("storage_item_get", Assert.Single(catalog.Commands).Key);
        Assert.NotNull(catalog.Resolve("storage_item_get"));
        Assert.Null(catalog.Resolve("item_get"));
        Assert.Null(catalog.Resolve("item_list"));
    }

    [Theory]
    [InlineData("storage_item_list")]
    [InlineData("STORAGE_ITEM_LIST")]
    [InlineData("item_list")]
    public void Create_RejectsCanonicalAndAliasCollisions(string conflictingName)
    {
        var command = Substitute.For<IBaseCommand>();
        command.Name.Returns("get");
        command.NamespaceOperationName.Returns("list");
        var registrations = new Dictionary<string, CommandRegistration>
        {
            ["storage_item_get"] = new(command, "storage"),
            [conflictingName] = new(CreateCommand(), "storage")
        };

        var exception = Assert.Throws<InvalidOperationException>(() =>
            NamespaceCommandCatalog.Create("storage", registrations, true));

        Assert.Contains("collides", exception.Message);
        Assert.Contains("storage_item_get", exception.Message);
        Assert.Contains(conflictingName, exception.Message);
    }

    [Fact]
    public void Create_RejectsAliasShadowingAnotherCanonicalName()
    {
        var registrations = new Dictionary<string, CommandRegistration>
        {
            ["storage_get"] = new(CreateCommand(), "storage"),
            ["storage_storage_get"] = new(CreateCommand(), "storage")
        };

        Assert.Throws<InvalidOperationException>(() =>
            NamespaceCommandCatalog.Create("storage", registrations, true));
    }

    [Fact]
    public void Create_RejectsOperationOverrideThatDoesNotMatchRegisteredPath()
    {
        var command = Substitute.For<IBaseCommand>();
        command.Name.Returns("get");
        command.NamespaceOperationName.Returns("list");

        Assert.Throws<InvalidOperationException>(() => NamespaceCommandCatalog.Create("storage",
            new Dictionary<string, CommandRegistration> { ["storage_item_show"] = new(command, "storage") }, true));
    }

    [Fact]
    public void Create_EmptyCatalogDoesNotResolveAnything()
    {
        var catalog = NamespaceCommandCatalog.Create("storage", new Dictionary<string, CommandRegistration>(), true);

        Assert.Empty(catalog.Commands);
        Assert.Null(catalog.Resolve(null));
        Assert.Null(catalog.Resolve("get"));
    }

    private static IBaseCommand CreateCommand()
    {
        var command = Substitute.For<IBaseCommand>();
        command.NamespaceOperationName.Returns((string?)null);
        return command;
    }
}
