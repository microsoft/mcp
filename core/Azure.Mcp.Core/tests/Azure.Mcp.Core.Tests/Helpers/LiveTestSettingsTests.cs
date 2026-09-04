// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public class LiveTestSettingsTests
{
    [Theory]
    [InlineData("Live", TestMode.Live)]
    [InlineData("Record", TestMode.Record)]
    [InlineData("Playback", TestMode.Playback)]
    [InlineData("playback", TestMode.Playback)]
    public void DeserializeTestMode_WithValidValue_ReturnsExpectedMode(string value, TestMode expected)
    {
        var settings = JsonSerializer.Deserialize<LiveTestSettings>($$"""{ "TestMode": "{{value}}" }""");

        Assert.NotNull(settings);
        Assert.Equal(expected, settings.TestMode);
    }

    [Fact]
    public void DeserializeTestMode_WithInvalidString_ThrowsActionableException()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LiveTestSettings>("""{ "TestMode": "Replay" }"""));

        Assert.Contains("Invalid TestMode 'Replay'. TestMode must be one of: Live, Record, Playback.", exception.Message);
    }

    [Fact]
    public void DeserializeTestMode_WithNumericString_ThrowsActionableException()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LiveTestSettings>("""{ "TestMode": "1" }"""));

        Assert.Contains("Invalid TestMode '1'. TestMode must be one of: Live, Record, Playback.", exception.Message);
    }

    [Fact]
    public void DeserializeTestMode_WithNonStringValue_ThrowsActionableException()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LiveTestSettings>("""{ "TestMode": 1 }"""));

        Assert.Contains("Invalid TestMode value. TestMode must be one of: Live, Record, Playback.", exception.Message);
    }
}