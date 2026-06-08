// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Sql.Services;
using Xunit;

namespace Azure.Mcp.Tools.Sql.Tests.Services;

public class SqlServiceTests
{
    [Fact]
    public void BuildServerResourceFilter_ScopesQueryToServer()
    {
        // Ensures list operations (databases, elastic pools) are filtered to a single SQL server
        // rather than returning every resource in the resource group (issues #448 and #452).
        var filter = SqlService.BuildServerResourceFilter("server1");

        Assert.Equal("id contains '/servers/server1/'", filter);
    }

    [Theory]
    [InlineData("my-server", "id contains '/servers/my-server/'")]
    [InlineData("o'brien", "id contains '/servers/o''brien/'")]
    [InlineData("back\\slash", "id contains '/servers/back\\\\slash/'")]
    public void BuildServerResourceFilter_EscapesServerNameForKql(string serverName, string expected)
    {
        var filter = SqlService.BuildServerResourceFilter(serverName);

        Assert.Equal(expected, filter);
    }

    [Fact]
    public void BuildServerResourceFilter_DoesNotContainPipeOperator()
    {
        // The base resource query rejects additional filters containing '|' to prevent KQL injection.
        var filter = SqlService.BuildServerResourceFilter("server1");

        Assert.DoesNotContain('|', filter);
    }
}
