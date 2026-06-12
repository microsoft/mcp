// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Commands;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Commands;

public sealed class CommandMetadataAttributeTests
{
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
            Title = title
        };
        Assert.Equal(expected, attr.IsValid());
    }
}
