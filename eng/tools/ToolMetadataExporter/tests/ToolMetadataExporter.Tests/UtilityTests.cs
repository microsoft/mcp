// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using Xunit;

namespace ToolMetadataExporter.Tests;

public class UtilityTests
{
    [Fact]
    public async Task LoadToolsDynamicallyAsync_EscapesUnescapedControlCharactersInJsonStrings()
    {
        const string ServerFile = "azmcp.exe";
        var output =
            """{"status":200,"message":"Success","results":[{"name":"test","description":"before"""
            + '\u001A'
            + """after","command":"test"}]}""";

        var logger = Substitute.For<ILogger<Utility>>();
        var utility = Substitute.ForPartsOf<Utility>(logger);
        utility.Configure()
            .ExecuteAzmcpAsync(ServerFile, "tools list", false, true)
            .Returns(Task.FromResult(output));

        var result = await utility.LoadToolsDynamicallyAsync(ServerFile, string.Empty);

        var tool = Assert.Single(result!.Tools!);
        Assert.Equal($"before{'\u001A'}after", tool.Description);
    }

    [Fact]
    public async Task LoadToolsDynamicallyAsync_EscapesWhitespaceControlCharactersInsideJsonStrings()
    {
        const string ServerFile = "azmcp.exe";
        const string Description = "before\tmiddle\r\nafter";
        var output =
            "{\r\n"
            + "  \"status\": 200, \"results\": [{\"name\":\"test\",\"description\":\""
            + Description
            + "\",\"command\":\"test\"}]}"
            + "\r\n";

        var logger = Substitute.For<ILogger<Utility>>();
        var utility = Substitute.ForPartsOf<Utility>(logger);
        utility.Configure()
            .ExecuteAzmcpAsync(ServerFile, "tools list", false, true)
            .Returns(Task.FromResult(output));

        var result = await utility.LoadToolsDynamicallyAsync(ServerFile, string.Empty);

        var tool = Assert.Single(result!.Tools!);
        Assert.Equal(Description, tool.Description);
    }

    [Fact]
    public async Task LoadToolsDynamicallyAsync_DoesNotMaskOtherInvalidJson()
    {
        const string ServerFile = "azmcp.exe";
        const string Output = """{"status": 200, "results": [}""";

        var logger = Substitute.For<ILogger<Utility>>();
        var utility = Substitute.ForPartsOf<Utility>(logger);
        utility.Configure()
            .ExecuteAzmcpAsync(ServerFile, "tools list", false, true)
            .Returns(Task.FromResult(Output));

        await Assert.ThrowsAsync<JsonException>(
            () => utility.LoadToolsDynamicallyAsync(ServerFile, string.Empty));
    }
}
