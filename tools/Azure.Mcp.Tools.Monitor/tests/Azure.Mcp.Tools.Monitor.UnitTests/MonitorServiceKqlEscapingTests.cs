// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Monitor.UnitTests;

public class MonitorServiceKqlEscapingTests
{
    [Fact]
    public void EscapeKqlIdentifier_SimpleName_WrapsInBrackets()
    {
        var result = KqlSanitizer.EscapeIdentifier("MyTable");

        Assert.Equal("['MyTable']", result);
    }

    [Fact]
    public void EscapeKqlIdentifier_NameWithSingleQuote_EscapesQuote()
    {
        var result = KqlSanitizer.EscapeIdentifier("My'Table");

        Assert.Equal("['My''Table']", result);
    }

    [Fact]
    public void EscapeKqlIdentifier_NameWithPipeOperator_SafelyWrapped()
    {
        var result = KqlSanitizer.EscapeIdentifier("table' | union OtherTable | where '1'=='1");

        Assert.Equal("['table'' | union OtherTable | where ''1''==''1']", result);
    }

    [Fact]
    public void EscapeKqlIdentifier_NormalTableName_WrapsCorrectly()
    {
        var result = KqlSanitizer.EscapeIdentifier("AppServiceConsoleLogs");

        Assert.Equal("['AppServiceConsoleLogs']", result);
    }

    [Fact]
    public void EscapeKqlIdentifier_NameWithSpaces_WrapsCorrectly()
    {
        var result = KqlSanitizer.EscapeIdentifier("My Table Name");

        Assert.Equal("['My Table Name']", result);
    }
}
