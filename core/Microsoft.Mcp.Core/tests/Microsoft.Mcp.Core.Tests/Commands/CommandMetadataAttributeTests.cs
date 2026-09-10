// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class CommandMetadataAttributeTests
{
    [Fact]
    public void CommandMetadataAttribute_AllPropertiesAreRequired()
    {
        var properties = typeof(CommandMetadataAttribute).GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        Assert.NotEmpty(properties);
        Assert.All(properties, property => Assert.True(
            property.IsDefined(typeof(RequiredMemberAttribute), inherit: false),
            $"{property.Name} must be required."));
    }

    [Theory]
    [InlineData("id1", "name1", "desc1", "title1", true)]
    [InlineData("", "name1", "desc1", "title1", false)]
    [InlineData("id1", "", "desc1", "title1", false)]
    [InlineData("id1", "name1", "", "title1", false)]
    [InlineData("id1", "name1", "desc1", "", false)]
    [InlineData("   ", "name1", "desc1", "title1", false)]
    [InlineData("id1", "   ", "desc1", "title1", false)]
    [InlineData("id1", "name1", "   ", "title1", false)]
    [InlineData("id1", "name1", "desc1", "   ", false)]
    public void CommandMetadataAttribute_IsValid(string id, string name, string description, string title, bool expected)
    {
        var attr = new CommandMetadataAttribute()
        {
            Id = id,
            Name = name,
            Description = description,
            Title = title,
            OperationPlane = ToolOperationPlane.Unspecified,
            Destructive = true,
            Idempotent = false,
            OpenWorld = true,
            ReadOnly = false,
            Secret = false,
            LocalRequired = false
        };
        Assert.Equal(expected, attr.IsValid());
    }
}
