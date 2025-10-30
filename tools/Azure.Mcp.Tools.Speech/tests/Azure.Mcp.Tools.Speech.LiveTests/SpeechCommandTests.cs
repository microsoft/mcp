﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests.Client;
using Azure.Mcp.Tools.Speech.Models;
using Azure.Mcp.Tools.Speech.Models.Realtime;
using Xunit;

namespace Azure.Mcp.Tools.Speech.LiveTests;

[Trait("Toolset", "Speech")]
[Trait("Category", "Live")]
public class SpeechCommandTests(ITestOutputHelper output) : CommandTestsBase(output)
{
    #region SpeechToText Tests

    [Fact]
    public async Task SpeechToText_ShouldHandleMissingAudioFileGracefully()
    {
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "speech_stt_recognize",
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

    [Theory]
    [InlineData(null, "test-audio.wav", "My voice is my passport. Verify me.")] // Fast Transcription without language will use multi-language model
    [InlineData("en-US", "test-audio.wav", "My voice is my passport. Verify me.")]
    [InlineData("en-US", "whatstheweatherlike.mp3", "What's the weather like?")]
    [InlineData("en-US", "TheGreatGatsby.wav", "In my younger and more vulnerable years, my father gave me some advice that I've been turning over in my mind ever since. Whenever you feel like criticizing anyone, he told me, just remember that all the people in this world haven't had the advantages that you've had. He didn't say anymore, but we've always been unusually commutative in a reserved way, and I understood that he meant a great deal more than that. In consequence, I'm inclined to reserve all judgments, a habit that has opened up many curious natures to me.")]
    [InlineData("ar-AE", "ar-rewind-music.wav", "ارجع الموسيقى 20 ثانية.")]
    [InlineData("es-ES", "es-ES.wav", "Rebobinar la música 20 segundos.")]
    [InlineData("fr-FR", "fr-FR.wav", "Rembobinez la musique de Vingt secondes.")]
    [InlineData("de-DE", "de-DE.wav", "Treffen heute um 17 Uhr.")]
    public async Task SpeechToText_WithFastSupportedLanguage_ShouldRecognizeSpeechWithFastTranscription(string? language, string fileName, string expectedText)
    {
        // Arrange
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);

        Assert.True(File.Exists(testAudioFile),
            $"Test audio file not found at: {testAudioFile}. Please ensure {fileName} exists in TestResources folder.");

        var fileInfo = new FileInfo(testAudioFile);
        Assert.True(fileInfo.Length > 0, "Test audio file must not be empty");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Act
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", language },
            });

        // Assert
        using var doc = JsonDocument.Parse(result.Value.GetRawText());
        var inner = doc.RootElement.GetProperty("result").GetRawText();
        var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
        Output.WriteLine("Recognition Result: " + result?.ToString());

        // STRICT REQUIREMENT: Speech recognition must return a result
        Assert.NotNull(resultObj);
        Assert.Equal(RecognizerType.Fast, resultObj.RecognizerType);
        Assert.NotNull(resultObj.FastTranscriptionResult);
        Assert.Null(resultObj.RealtimeContinuousResult);
        Assert.Equal(expectedText, resultObj.FastTranscriptionResult.CombinedPhrases?.FirstOrDefault()?.Text);
    }

    [Theory]
    [InlineData("af-ZA", "af-ZA.wav", "Hoe lyk die weer?")]
    [InlineData("fr-CA", "fr-CA.wav", "Quel temps fait-il?")]
    [InlineData("ar-DZ", "ar-DZ.wav", "أنا ذاهب إلى السوق لأشتري بعض الفواكه والخضروات الطازجة.")]
    public async Task SpeechToText_WithFastUnsupportedLanguage_ShouldFallBackToRealtimeTranscription(string language, string fileName, string expectedText)
    {
        // Arrange
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);

        Assert.True(File.Exists(testAudioFile),
            $"Test audio file not found at: {testAudioFile}. Please ensure {fileName} exists in TestResources folder.");

        var fileInfo = new FileInfo(testAudioFile);
        Assert.True(fileInfo.Length > 0, "Test audio file must not be empty");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Act
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", language },
            });

        // Assert
        using var doc = JsonDocument.Parse(result.Value.GetRawText());
        var inner = doc.RootElement.GetProperty("result").GetRawText();
        var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
        Output.WriteLine("Recognition Result: " + result?.ToString());

        Assert.NotNull(resultObj);
        Assert.Equal(RecognizerType.Realtime, resultObj.RecognizerType);
        Assert.Null(resultObj.FastTranscriptionResult);
        Assert.NotNull(resultObj.RealtimeContinuousResult);
        Assert.Equal(expectedText, resultObj.RealtimeContinuousResult.FullText);
        Assert.True(resultObj.RealtimeContinuousResult.Segments.Count > 0);
        // Assert each segment has the expected reason
        Assert.All(resultObj.RealtimeContinuousResult.Segments, segment =>
        {
            Assert.Equal("RecognizedSpeech", segment.Reason);
        });
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("detailed")]
    public async Task SpeechToText_WithFormat_ShouldRecognizeSpeechWithRealtimeTranscription(string format)
    {
        // Arrange
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var expectedText = "By voice is my passport. Verify me.";

        // Act
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", format }
            });

        // Assert
        using var doc = JsonDocument.Parse(result.Value.GetRawText());
        var inner = doc.RootElement.GetProperty("result").GetRawText();
        var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
        Output.WriteLine("Recognition Result: " + result?.ToString());

        Assert.NotNull(resultObj);
        Assert.Equal(RecognizerType.Realtime, resultObj.RecognizerType);
        Assert.Null(resultObj.FastTranscriptionResult);
        Assert.NotNull(resultObj.RealtimeContinuousResult);
        Assert.Equal(expectedText, resultObj.RealtimeContinuousResult.FullText);
        Assert.True(resultObj.RealtimeContinuousResult.Segments.Count > 0);
        // Assert each segment has the expected reason
        Assert.All(resultObj.RealtimeContinuousResult.Segments, segment =>
        {
            Assert.Equal("RecognizedSpeech", segment.Reason);
        });

        if (format == "detailed")
        {
            // detailed format should have NBest alternatives in each segment
            Assert.All(resultObj.RealtimeContinuousResult.Segments, segment =>
            {
                if (segment is RealtimeRecognitionDetailedResult detailedSegment)
                {
                    Assert.NotNull(detailedSegment.NBest);
                    Assert.True(detailedSegment.NBest.Count > 0, "Each segment should have NBest alternatives in detailed format");
                }
                else
                {
                    Assert.Fail("Segment should be of type RealtimeRecognitionDetailedResult in detailed format");
                }
            });
        }
    }

    [Theory]
    [InlineData("masked", "simple", "You don't deserve it, you *******. **** you.")]
    [InlineData("removed", "simple", "You don't deserve it, you .  you.")]
    [InlineData("raw", "simple", "You don't deserve it, you bastard. Fuck you.")]
    public async Task SpeechToText_WithDifferentProfanityOptions_ShouldApplyCorrectly(string profanityOption, string format, string expectedText)
    {
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "en-US-with-profanity.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", format },
                { "profanity", profanityOption }
            });

        using var doc = JsonDocument.Parse(result.Value.GetRawText());
        var inner = doc.RootElement.GetProperty("result").GetRawText();
        var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
        Output.WriteLine("Recognition Result: " + result?.ToString());

        Assert.NotNull(resultObj);
        Assert.Equal(RecognizerType.Realtime, resultObj.RecognizerType);
        Assert.Null(resultObj.FastTranscriptionResult);
        Assert.NotNull(resultObj.RealtimeContinuousResult);
        Assert.Equal(expectedText, resultObj.RealtimeContinuousResult.FullText);
        Assert.True(resultObj.RealtimeContinuousResult.Segments.Count > 0);
        // Assert each segment has the expected reason
        Assert.All(resultObj.RealtimeContinuousResult.Segments, segment =>
        {
            Assert.Equal("RecognizedSpeech", segment.Reason);
        });
    }

    [Fact]
    public async Task SpeechToText_WithPhrases_ShouldIncreaseRecognitionAccuracy()
    {
        // Arrange
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "en-US-phraselist.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var expectedText = "Years later, Douzi and Shitou have become packing opera stars, taking the names Cheng Dieyi and Duan Xiaolou, respectively.";

        // Act
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "phrases", new[] { "Douzi", "Shitou", "Cheng Dieyi", "Duan Xiaolou" } }
            });

        // Assert
        using var doc = JsonDocument.Parse(result.Value.GetRawText());
        var inner = doc.RootElement.GetProperty("result").GetRawText();
        var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
        Output.WriteLine("Recognition Result: " + result?.ToString());

        // STRICT REQUIREMENT: Speech recognition must return a result
        Assert.NotNull(resultObj);
        Assert.Equal(RecognizerType.Fast, resultObj.RecognizerType);
        Assert.NotNull(resultObj.FastTranscriptionResult);
        Assert.Null(resultObj.RealtimeContinuousResult);
        Assert.Equal(expectedText, resultObj.FastTranscriptionResult.CombinedPhrases?.FirstOrDefault()?.Text);
    }

    [Fact]
    public async Task SpeechToText_WithInvalidEndpoint_ShouldHandleGracefully()
    {
        // Arrange
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var invalidEndpoint = "https://invalid-endpoint.cognitiveservices.azure.com/";
        // Act
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", invalidEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" }
            });

        // Assert
        Assert.NotNull(result);
        var resultText = result.ToString();
        Output.WriteLine("Recognition Result: " + resultText);
        Assert.NotNull(resultText);
        Assert.Contains("Invalid endpoint or connectivity issue.", resultText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SpeechToText_WithEmptyAudioFileAndFastTranscription_ShouldHandleGracefully()
    {
        // Create a valid empty WAV file
        var emptyWavFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        CreateWavFile(emptyWavFile);

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", emptyWavFile },
                    { "language", "en-US" },
                });

            // Assert
            using var doc = JsonDocument.Parse(result.Value.GetRawText());
            var inner = doc.RootElement.GetProperty("result").GetRawText();
            var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
            Output.WriteLine("Recognition Result: " + result?.ToString());

            // STRICT REQUIREMENT: Speech recognition must return a result
            Assert.NotNull(resultObj);
            Assert.Equal(RecognizerType.Fast, resultObj.RecognizerType);
            Assert.NotNull(resultObj.FastTranscriptionResult);
            Assert.Null(resultObj.RealtimeContinuousResult);
            Assert.Empty(resultObj.FastTranscriptionResult.CombinedPhrases);
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(emptyWavFile))
            {
                File.Delete(emptyWavFile);
            }
        }
    }

    [Fact]
    public async Task SpeechToText_WithEmptyAudioFileAndRealtimeTranscription_ShouldHandleGracefully()
    {
        // Create a valid empty WAV file
        var emptyWavFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        CreateWavFile(emptyWavFile);

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            var result = await CallToolAsync(
                "speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", emptyWavFile },
                    { "language", "en-US" },
                    { "format", "simple" }
                });

            // Assert
            using var doc = JsonDocument.Parse(result.Value.GetRawText());
            var inner = doc.RootElement.GetProperty("result").GetRawText();
            var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
            Output.WriteLine("Recognition Result: " + result?.ToString());

            Assert.NotNull(resultObj);
            Assert.Equal(RecognizerType.Realtime, resultObj.RecognizerType);
            Assert.Null(resultObj.FastTranscriptionResult);
            Assert.NotNull(resultObj.RealtimeContinuousResult);
            Assert.True(string.IsNullOrEmpty(resultObj.RealtimeContinuousResult.FullText));
            Assert.True(resultObj.RealtimeContinuousResult.Segments.Count > 0);
            // Assert each segment has the expected reason
            Assert.All(resultObj.RealtimeContinuousResult.Segments, segment =>
            {
                Assert.Equal("NoMatch", segment.Reason);
            });
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(emptyWavFile))
            {
                File.Delete(emptyWavFile);
            }
        }
    }

    [Fact]
    public async Task SpeechToText_WithBrokenFile_ShouldHandleGracefully()
    {
        // Arrange
        var brokenWavFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        File.WriteAllText(brokenWavFile, "123"); // Broken audio content

        try
        {
            var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

            // Act
            var result = await CallToolAsync(
                "speech_stt_recognize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "file", brokenWavFile },
                    { "language", "en-US" },
                });

            // Assert
            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);
            Output.WriteLine("Recognition Result: " + result?.ToString());

            // Parse to ensure valid JSON structure
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;

            // Validate Error message for corrupted file
            Assert.True(resultObject.TryGetProperty("message", out var messageProperty));
            var message = messageProperty.GetString() ?? "";
            Assert.True(message.Contains("The audio file appears to be empty or corrupted. Please provide a valid audio file.", StringComparison.OrdinalIgnoreCase));

            // Validate exception type
            Assert.True(resultObject.TryGetProperty("type", out var exceptionTypeProperty));
            var exceptionType = exceptionTypeProperty.GetString() ?? "";
            Assert.True(exceptionType.Contains("InvalidOperationException", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(brokenWavFile))
            {
                File.Delete(brokenWavFile);
            }
        }
    }

    [Fact]
    public async Task SpeechToText_ShouldHandleRetryPolicyCorrectly()
    {
        // Arrange
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", "test-audio.wav");
        Assert.True(File.Exists(testAudioFile), $"Test audio file not found at: {testAudioFile}");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var expectedText = "My voice is my passport. Verify me.";

        // Act
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "retry-max-retries", 3 },
                { "retry-delay", 1000 }
            });

        // Assert
        using var doc = JsonDocument.Parse(result.Value.GetRawText());
        var inner = doc.RootElement.GetProperty("result").GetRawText();
        var resultObj = JsonSerializer.Deserialize<SpeechRecognitionResult>(inner);
        Output.WriteLine("Recognition Result: " + result?.ToString());

        // STRICT REQUIREMENT: Speech recognition must return a result
        Assert.NotNull(resultObj);
        Assert.Equal(RecognizerType.Fast, resultObj.RecognizerType);
        Assert.NotNull(resultObj.FastTranscriptionResult);
        Assert.Null(resultObj.RealtimeContinuousResult);
        Assert.Equal(expectedText, resultObj.FastTranscriptionResult.CombinedPhrases?.FirstOrDefault()?.Text);
    }

    [Fact]
    public async Task SpeechToText_RecognizeCompressedAudioWithRealtimeTranscription_ShouldFailWithoutGStreamer()
    {
        // This test validates speech recognition with different audio file formats
        var fileName = "whatstheweatherlike.mp3";
        var testAudioFile = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "TestResources", fileName);

        // STRICT REQUIREMENT: The test audio file MUST exist in TestResources
        Assert.True(File.Exists(testAudioFile),
            $"Test audio file not found at: {testAudioFile}. Please ensure {fileName} exists in TestResources folder.");

        var fileInfo = new FileInfo(testAudioFile);
        Assert.True(fileInfo.Length > 0, "Test audio file must not be empty");

        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";

        // Test with the audio file - expect successful speech recognition
        var result = await CallToolAsync(
            "speech_stt_recognize",
            new()
            {
                { "subscription", Settings.SubscriptionId },
                { "endpoint", aiServicesEndpoint },
                { "file", testAudioFile },
                { "language", "en-US" },
                { "format", "simple" }
            });

        // Should handle empty file gracefully
        Assert.NotNull(result);
        var resultText = result.ToString();
        Assert.NotNull(resultText);

        // Parse to ensure valid JSON structure
        var jsonResult = JsonDocument.Parse(resultText);
        var resultObject = jsonResult.RootElement;

        // Validate Error message for corrupted file
        Assert.True(resultObject.TryGetProperty("message", out var messageProperty));
        var message = messageProperty.GetString() ?? "";
        Assert.True(message.Contains("Cannot process compressed audio file", StringComparison.OrdinalIgnoreCase));
        Assert.True(message.Contains("because GStreamer is not properly installed or configured.", StringComparison.OrdinalIgnoreCase));

        // Validate exception type
        Assert.True(resultObject.TryGetProperty("type", out var exceptionTypeProperty));
        var exceptionType = exceptionTypeProperty.GetString() ?? "";
        Assert.True(exceptionType.Contains("InvalidOperationException", StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    /// <summary>
    /// Create a WAV file with given duration (seconds).
    /// If durationSeconds = 0, generates an empty WAV file with header only.
    /// </summary>
    private static void CreateWavFile(string filePath, int durationSeconds = 0)
    {
        int sampleRate = 16000;    // 16kHz
        short bitsPerSample = 16;  // 16-bit
        short channels = 1;        // mono
        int totalSamples = sampleRate * durationSeconds;
        int byteRate = sampleRate * channels * (bitsPerSample / 8);

        using var fs = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(fs);

        // RIFF header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + totalSamples * 2); // RIFF size
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);          // PCM chunk size
        writer.Write((short)1);    // PCM format
        writer.Write(channels);    // channels
        writer.Write(sampleRate);  // sample rate
        writer.Write(byteRate);    // byte rate
        writer.Write((short)(channels * bitsPerSample / 8)); // block align
        writer.Write(bitsPerSample);

        // data chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(totalSamples * 2); // data chunk size

        // Write silence (zeros) for the specified duration
        for (int i = 0; i < totalSamples; i++)
        {
            writer.Write((short)0);
        }
    }
}

