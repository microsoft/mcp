// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Xunit;

namespace Azure.Mcp.Server.UnitTests.Infrastructure;

public class GetTargetAreaNameTests
{
    // -----------------------------------------------------------------------
    // Branch 1: No first non-option token — returns null (full init path)
    // -----------------------------------------------------------------------

    public static TheoryData<string[]> NoNonOptionTokenCases => new()
    {
        { [] },                                              // empty args
        { ["--help"] },                                      // only a flag
        { ["--learn"] },                                     // only --learn
        { ["--version"] },                                   // only --version
        { ["-v"] },                                          // short alias
        { ["--"] },                                          // end-of-options only
        { ["--", "storage"] },                               // token is after --, must be ignored
        // A global option whose value is a known area name still returns null because
        // the option value ("sub1") precedes the area token and is not itself an area.
        { ["--subscription", "sub1", "storage", "account", "list"] },
    };

    [Theory]
    [MemberData(nameof(NoNonOptionTokenCases))]
    public void GetTargetAreaName_NoNonOptionToken_ReturnsNull(string[] args)
    {
        Assert.Null(Program.GetTargetAreaName(args));
    }

    // -----------------------------------------------------------------------
    // Branch 2: "tools" special case — always returns null
    // -----------------------------------------------------------------------

    public static TheoryData<string[]> ToolsTokenCases => new()
    {
        { ["tools"] },
        { ["tools", "list"] },
        { ["TOOLS"] },          // case-insensitive
    };

    [Theory]
    [MemberData(nameof(ToolsTokenCases))]
    public void GetTargetAreaName_ToolsToken_ReturnsNull(string[] args)
    {
        Assert.Null(Program.GetTargetAreaName(args));
    }

    // -----------------------------------------------------------------------
    // Branch 3: Unknown / unregistered token — returns null (typo fallback)
    // -----------------------------------------------------------------------

    public static TheoryData<string[]> UnknownTokenCases => new()
    {
        { ["notanarea"] },
        { ["storagee"] },       // typo
        { ["xyz123"] },
    };

    [Theory]
    [MemberData(nameof(UnknownTokenCases))]
    public void GetTargetAreaName_UnknownToken_ReturnsNull(string[] args)
    {
        Assert.Null(Program.GetTargetAreaName(args));
    }

    // -----------------------------------------------------------------------
    // Branch 4: Happy path — known area token, returned verbatim
    // -----------------------------------------------------------------------

    public static TheoryData<string[], string> KnownAreaTokenCases => new()
    {
        { ["storage"], "storage" },
        { ["STORAGE"], "STORAGE" },                     // casing preserved
        { ["server"], "server" },
        { ["storage", "account", "list"], "storage" },
        // When the first token is an option flag and the second token is a known area name,
        // the area name is correctly detected (e.g. "--namespace storage" for server mode).
        { ["--namespace", "storage"], "storage" },
    };

    [Theory]
    [MemberData(nameof(KnownAreaTokenCases))]
    public void GetTargetAreaName_KnownAreaToken_ReturnsToken(string[] args, string expected)
    {
        Assert.Equal(expected, Program.GetTargetAreaName(args));
    }
}
