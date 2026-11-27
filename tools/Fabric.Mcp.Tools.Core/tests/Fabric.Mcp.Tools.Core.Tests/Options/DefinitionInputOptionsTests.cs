// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Fabric.Mcp.Tools.Core.Options;
using Microsoft.Mcp.Core.Options;
using Xunit;

namespace Fabric.Mcp.Tools.Core.Tests.Options;

public sealed class DefinitionInputOptionsTests()
{
    [Fact]
    public static void Descriptors_ComposeOnlyOptionalSourceFieldsWithoutChangingFlatNames()
    {
        var shared = OptionDescriptor.FromType<DefinitionInputOptions>();
        var create = OptionDescriptor.FromType<ItemCreateOptions>();
        var composed = create.Where(static option => option.ParentType == typeof(DefinitionInputOptions)).ToArray();

        Assert.Equal(["definition", "definition-directory", "definition-files", "definition-format"],
            shared.Select(static option => option.Name).Order());
        Assert.Equal(shared.Select(static option => option.Name).Order(), composed.Select(static option => option.Name).Order());
        Assert.All(composed, static option =>
        {
            Assert.Equal(nameof(ItemCreateOptions.DefinitionInput), option.ParentProperty?.Name);
            Assert.Equal(typeof(string), option.Type);
            Assert.False(option.Required);
            Assert.Null(option.DefaultValue);
            Assert.DoesNotContain("creation-payload", option.Description, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("create-request", option.Description, StringComparison.OrdinalIgnoreCase);
        });
        foreach (var name in new[] { "creation-payload", "folder-id", "options", "sensitivity-label-settings" })
        {
            Assert.Null(Assert.Single(create, option => option.Name == name).ParentProperty);
        }
    }

    [Fact]
    public static void Binding_OmittedGroupStaysNullAndStandaloneInputsRemainOptional()
    {
        var command = new Command("create-item");
        OptionBinder.RegisterOptions<ItemCreateOptions>(command);
        var options = OptionBinder.BindOptions<ItemCreateOptions>(command.Parse(
            ["--display-name", "Created", "--item-type", "Notebook"]));
        var sharedCommand = new Command("definition-input");
        OptionBinder.RegisterOptions<DefinitionInputOptions>(sharedCommand);
        var shared = OptionBinder.BindOptions<DefinitionInputOptions>(sharedCommand.Parse([]));

        Assert.Null(options.DefinitionInput);
        Assert.False(shared.HasInput);
        Assert.Null(shared.Definition);
        Assert.Null(shared.DefinitionDirectory);
        Assert.Null(shared.DefinitionFiles);
        Assert.Null(shared.DefinitionFormat);
        Assert.All(sharedCommand.Options, static option => Assert.False(option.Required));
    }

    [Theory]
    [InlineData("--definition")]
    [InlineData("--definition-directory")]
    [InlineData("--definition-files")]
    [InlineData("--definition-format")]
    public static void Binding_AllocatesIndependentGroupsForEachInvocation(string name)
    {
        var command = new Command("create-item");
        OptionBinder.RegisterOptions<ItemCreateOptions>(command);
        var first = OptionBinder.BindOptions<ItemCreateOptions>(command.Parse(
            ["--display-name", "First", "--item-type", "Notebook", name, "first-value"]));
        var second = OptionBinder.BindOptions<ItemCreateOptions>(command.Parse(
            ["--display-name", "Second", "--item-type", "Notebook", name, "second-value"]));

        Assert.NotNull(first.DefinitionInput);
        Assert.NotNull(second.DefinitionInput);
        Assert.NotSame(first.DefinitionInput, second.DefinitionInput);
        Assert.True(first.DefinitionInput.HasInput);
        Assert.True(second.DefinitionInput.HasInput);
        Assert.Equal("first-value", SelectedValue(first.DefinitionInput, name));
        Assert.Equal("second-value", SelectedValue(second.DefinitionInput, name));
    }

    private static string? SelectedValue(DefinitionInputOptions options, string name) => name switch
    {
        "--definition" => options.Definition,
        "--definition-directory" => options.DefinitionDirectory,
        "--definition-files" => options.DefinitionFiles,
        "--definition-format" => options.DefinitionFormat,
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };
}
