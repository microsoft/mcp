// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Xunit;

namespace Microsoft.Mcp.Core.Tests.Helpers;

public sealed class PaginationCursorTests()
{
    [Fact]
    public void Cursor_RoundTripsOpaqueValueWithTheSameArguments()
    {
        const string serviceCursor = " opaque. +/=%2F\u00e9 ";
        var encoded = new PaginationCursor("tool", "account", null, "private-prefix").Encode(serviceCursor);
        Assert.NotNull(encoded);
        Assert.DoesNotContain("private-prefix", encoded);

        var continuation = new PaginationCursor("tool", "account", null, "private-prefix");
        Assert.Equal(serviceCursor, continuation.Decode(encoded));
    }

    [Theory]
    [InlineData("other-tool", "prefix")]
    [InlineData("tool", null)]
    [InlineData("tool", "Prefix")]
    public void Cursor_RejectsMismatchedToolOrArguments(string tool, string? prefix)
    {
        var encoded = new PaginationCursor("tool", "account", "prefix").Encode("service-cursor");
        var continuation = new PaginationCursor(tool, "account", prefix);

        var error = Assert.Throws<CommandValidationException>(() => continuation.Decode(encoded));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.Contains("does not match the original request", error.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t")]
    public void Encode_LeavesTerminalCursorAbsent(string? serviceCursor)
    {
        Assert.Null(new PaginationCursor("tool").Encode(serviceCursor));
    }

    [Fact]
    public void Decode_RejectsMalformedPaginationCursor()
    {
        var continuation = new PaginationCursor("tool");
        var encoded = continuation.Encode("service-cursor");
        Assert.NotNull(encoded);

        var error = Assert.Throws<CommandValidationException>(() => continuation.Decode(encoded + "!"));
        Assert.Equal(HttpStatusCode.BadRequest, error.StatusCode);
        Assert.DoesNotContain(encoded, error.Message);
    }
}
