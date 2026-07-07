// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

/// <summary>
/// Tests for <see cref="CommandMetadataAttribute"/>, <see cref="CommandMetadataAttribute.ToToolMetadata"/>,
/// and the metadata validation logic in <see cref="BaseCommand{TOptions}"/>.
/// </summary>
public sealed class BaseCommandMetadataTests
{
    // ---------- Minimal concrete command fixtures ----------

    [CommandMetadata(
        Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
        Name = "test-attribute",
        Title = "Test Attribute Command",
        Description = "A command used only in tests.",
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        Secret = true,
        LocalRequired = true)]
    private sealed class AttributeBasedCommand : BaseCommand<EmptyOptions>
    {
        protected override EmptyOptions BindOptions(ParseResult parseResult) => new();

        public override Task<CommandResponse> ExecuteAsync(
            CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
            => Task.FromResult(context.Response);
    }

    private sealed class NoMetadataCommand : BaseCommand<EmptyOptions>
    {
        protected override EmptyOptions BindOptions(ParseResult parseResult) => new();

        public override Task<CommandResponse> ExecuteAsync(
            CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
            => Task.FromResult(context.Response);
    }

    // ---------- Attribute-based metadata tests ----------

    [Fact]
    public void AttributeBasedCommand_PopulatesId()
    {
        var command = new AttributeBasedCommand();
        Assert.Equal("a1b2c3d4-e5f6-7890-abcd-ef1234567890", command.Id);
    }

    [Fact]
    public void AttributeBasedCommand_PopulatesName()
    {
        var command = new AttributeBasedCommand();
        Assert.Equal("test-attribute", command.Name);
    }

    [Fact]
    public void AttributeBasedCommand_PopulatesDescription()
    {
        var command = new AttributeBasedCommand();
        Assert.Equal("A command used only in tests.", command.Description);
    }

    [Fact]
    public void AttributeBasedCommand_PopulatesTitle()
    {
        var command = new AttributeBasedCommand();
        Assert.Equal("Test Attribute Command", command.Title);
    }

    [Fact]
    public void AttributeBasedCommand_PopulatesMetadata()
    {
        var command = new AttributeBasedCommand();
        Assert.NotNull(command.Metadata);
    }

    // ---------- ToToolMetadata mapping tests ----------

    [Fact]
    public void ToToolMetadata_MapsDestructive()
    {
        var command = new AttributeBasedCommand();
        Assert.False(command.Metadata.Destructive);
    }

    [Fact]
    public void ToToolMetadata_MapsIdempotent()
    {
        var command = new AttributeBasedCommand();
        Assert.True(command.Metadata.Idempotent);
    }

    [Fact]
    public void ToToolMetadata_MapsOpenWorld()
    {
        var command = new AttributeBasedCommand();
        Assert.False(command.Metadata.OpenWorld);
    }

    [Fact]
    public void ToToolMetadata_MapsReadOnly()
    {
        var command = new AttributeBasedCommand();
        Assert.True(command.Metadata.ReadOnly);
    }

    [Fact]
    public void ToToolMetadata_MapsSecret()
    {
        var command = new AttributeBasedCommand();
        Assert.True(command.Metadata.Secret);
    }

    [Fact]
    public void ToToolMetadata_MapsLocalRequired()
    {
        var command = new AttributeBasedCommand();
        Assert.True(command.Metadata.LocalRequired);
    }

    [Fact]
    public void ToToolMetadata_DefaultValues_AreCorrect()
    {
        // A fresh attribute with only required properties should use spec defaults:
        // Destructive=true, Idempotent=false, OpenWorld=true, ReadOnly=false, Secret=false, LocalRequired=false
        var attr = new CommandMetadataAttribute
        {
            Id = "00000000-0000-0000-0000-000000000000",
            Name = "default-test",
            Title = "Default Test",
            Description = "Checks attribute defaults."
        };
        var metadata = attr.ToToolMetadata();

        Assert.True(metadata.Destructive);
        Assert.False(metadata.Idempotent);
        Assert.True(metadata.OpenWorld);
        Assert.False(metadata.ReadOnly);
        Assert.False(metadata.Secret);
        Assert.False(metadata.LocalRequired);
    }

    // ---------- Missing metadata throws InvalidOperationException ----------

    [Fact]
    public void NoMetadataCommand_ExceptionMessage_MentionsAttribute()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => new NoMetadataCommand());
        Assert.Contains($"Command type '{typeof(NoMetadataCommand).FullName}' is missing required [CommandMetadata] attribute.", ex.Message);
    }
}
