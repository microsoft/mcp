// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Microsoft.Mcp.Tests.Client.Helpers;
using Microsoft.Mcp.Tests.Helpers;
using Xunit;

namespace Azure.Mcp.Core.Tests.Helpers;

public class LiveTestSettingsTests
{
    [Fact]
    public void TryFindTestSettingsFile_SearchesProjectParents()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"McpSettings-{Guid.NewGuid():N}");
        try
        {
            string projectDirectory = Path.Combine(directory, "tests", "project");
            Directory.CreateDirectory(projectDirectory);
            string settingsPath = Path.Combine(directory, "tests", LiveTestSettings.TestSettingsFileName);
            File.WriteAllText(settingsPath, "{}");

            Assert.True(LiveTestSettings.TryFindTestSettingsFile(out var path, projectDirectory));
            Assert.Equal(settingsPath, path);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void TestHost_PreservesSourcePathWhenOutputIsShared()
    {
        var sourcePath = System.Reflection.Assembly.GetEntryAssembly()?
            .GetCustomAttributes(typeof(System.Reflection.AssemblyMetadataAttribute), inherit: false)
            .Cast<System.Reflection.AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "SourcePath").Value;

        Assert.NotNull(sourcePath);
        Assert.True(File.Exists(Path.Combine(sourcePath, "Azure.Mcp.Core.Tests.csproj")));
        Assert.NotEqual(Path.GetFullPath(sourcePath), Path.GetFullPath(AppContext.BaseDirectory).TrimEnd(Path.DirectorySeparatorChar));
    }

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
    public void DeserializeTestMode_WithNonStringValue_ThrowsActionableException()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<LiveTestSettings>("""{ "TestMode": 1 }"""));

        Assert.Contains("Invalid TestMode value. TestMode must be one of: Live, Record, Playback.", exception.Message);
    }
}
