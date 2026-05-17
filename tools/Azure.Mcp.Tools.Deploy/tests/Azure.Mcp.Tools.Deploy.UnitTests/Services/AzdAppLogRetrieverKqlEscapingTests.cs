// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Deploy.UnitTests.Services;

public class AzdAppLogRetrieverKqlEscapingTests
{
    [Fact]
    public void EscapeKqlStringValue_PlainValue_ReturnsUnchanged()
    {
        var result = KqlSanitizer.EscapeStringValue("mycontainerapp");

        Assert.Equal("mycontainerapp", result);
    }

    [Fact]
    public void EscapeKqlStringValue_ValueWithSingleQuote_DoublesQuote()
    {
        var result = KqlSanitizer.EscapeStringValue("app'name");

        Assert.Equal("app''name", result);
    }

    [Fact]
    public void EscapeKqlStringValue_ValueWithMultipleQuotes_DoublesAllQuotes()
    {
        var result = KqlSanitizer.EscapeStringValue("it's a 'test'");

        Assert.Equal("it''s a ''test''", result);
    }

    [Fact]
    public void EscapeKqlStringValue_EmptyString_ReturnsEmpty()
    {
        var result = KqlSanitizer.EscapeStringValue("");

        Assert.Equal("", result);
    }

    [Fact]
    public void EscapeKqlStringValue_ValueWithPipeOperator_PreservesContent()
    {
        var result = KqlSanitizer.EscapeStringValue("value' | union malicious");

        Assert.Equal("value'' | union malicious", result);
    }
}
