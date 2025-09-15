// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Azure.Mcp.Tests;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tests.Client.Helpers;
using Xunit;

namespace Azure.Mcp.Tools.Speech.LiveTests;

[Trait("Toolset", "Speech")]
[Trait("Category", "Live")]
public class SpeechCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    [Fact]
    public async Task Should_handle_missing_audio_file_gracefully()
    {
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", "non-existent-test-audio.wav" }, // Intentionally non-existent for testing
                { "language", "en-US" },
                { "format", "simple" }
            });

        Assert.Null(result);
    }

    [Fact]
    public async Task Should_recognize_speech_from_real_audio_file()
    {
        // This test validates complete end-to-end speech recognition with the test-audio.wav file from TestResources
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        
        // STRICT REQUIREMENT: The test audio file MUST exist in TestResources
        Assert.True(File.Exists(testAudioFile), 
            $"Test audio file not found at: {testAudioFile}. Please ensure test-audio.wav exists in TestResources folder.");

        var fileInfo = new FileInfo(testAudioFile);
        Output.WriteLine($"Using test audio file: {testAudioFile}");
        Output.WriteLine($"Test audio file size: {fileInfo.Length} bytes");
        Assert.True(fileInfo.Length > 0, "Test audio file must not be empty");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Test with the real audio file - expect successful speech recognition
        var result = await CallToolAsync(
            "azmcp_speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" }
            });

        // STRICT REQUIREMENT: Speech recognition must return a result
        Assert.NotNull(result);
        
        var resultText = result.ToString();
        Output.WriteLine($"Speech recognition result: {resultText}");
        
        // Validate the result structure
        Assert.NotNull(resultText);
        Assert.NotEmpty(resultText);
        
        // Parse the JSON result to check the recognition outcome
        try
        {
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;
            
            if (resultObject.TryGetProperty("result", out var resultProperty))
            {
                var reason = resultProperty.TryGetProperty("reason", out var reasonProperty) 
                    ? reasonProperty.GetString() 
                    : "Unknown";
                    
                var text = resultProperty.TryGetProperty("text", out var textProperty) 
                    ? textProperty.GetString() 
                    : "";
                
                Output.WriteLine($"Recognition reason: {reason}");
                Output.WriteLine($"Recognition text: '{text}'");
                
                // For a real audio file, we expect the specific recognized text
                var expectedText = "By voice is my passport. Verify me.";
                
                // Assert that we got the exact expected text from the test audio file
                Assert.Equal(expectedText, text);
                
                Output.WriteLine($"✅ Successfully recognized expected speech: '{text}'");
            }
            else
            {
                Assert.Fail("Result JSON does not contain expected 'result' property");
            }
        }
        catch (JsonException ex)
        {
            Assert.Fail($"Failed to parse speech recognition result as JSON: {ex.Message}");
        }

        Output.WriteLine("✅ Speech recognition completed successfully with real audio file");
    }
}
