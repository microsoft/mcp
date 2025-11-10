// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json;
using Azure.Mcp.Tests;
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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", fileName);

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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", fileName);

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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", "test-audio.wav");
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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", "en-US-with-profanity.wav");
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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", "en-US-phraselist.wav");
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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", "test-audio.wav");
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
        Assert.NotNull(resultText);
        Assert.Contains("Invalid endpoint or connectivity issue.", resultText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SpeechToText_WithEmptyAudioFileAndFastTranscription_ShouldHandleGracefully()
    {
        // Create a valid empty WAV file
        var emptyWavFile = Path.Join(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
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
        var emptyWavFile = Path.Join(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
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
        var brokenWavFile = Path.Join(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
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
            Output.WriteLine("Recognition Result: " + resultText);

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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", "test-audio.wav");
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
        var testAudioFile = Path.Join(AppContext.BaseDirectory, "TestResources", fileName);

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

    #region TTS Synthesize Tests

    [Fact]
    public async Task Should_synthesize_speech_to_file_with_text()
    {
        // Test basic TTS synthesis with text input
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var outputFile = Path.Combine(Path.GetTempPath(), $"tts-test-{Guid.NewGuid()}.wav");

        try
        {
            var result = await CallToolAsync(
                "speech_tts_synthesize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "text", "Hello, this is a test of text to speech synthesis." },
                    { "outputAudio", outputFile },
                    { "language", "en-US" }
                });

            // Verify successful response
            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);

            // Parse and validate the JSON result
            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;
            var resultProperty = resultObject.AssertProperty("result");

            // Verify file path
            var filePathProperty = resultProperty.AssertProperty("filePath");
            Assert.Equal(outputFile, filePathProperty.GetString());

            var audioLengthProperty = resultProperty.AssertProperty("audioSize");
            Assert.True(audioLengthProperty.GetInt64() > 0);

            // Verify the output file was created and has content
            Assert.True(File.Exists(outputFile), $"Output file not created at: {outputFile}");
            var fileInfo = new FileInfo(outputFile);
            Assert.True(fileInfo.Length > 0, "Output file should not be empty");
        }
        finally
        {
            // Clean up
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Theory]
    [InlineData("en-US", "en-US-JennyNeural")]
    [InlineData("zh-CN", "zh-CN-XiaoxiaoNeural")]
    [InlineData("ja-JP", "ja-JP-NanamiNeural")]
    public async Task Should_synthesize_speech_with_different_voices(string language, string voice)
    {
        // Test TTS synthesis with different language/voice combinations
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var outputFile = Path.Combine(Path.GetTempPath(), $"tts-test-{language}-{Guid.NewGuid()}.wav");

        try
        {
            var result = await CallToolAsync(
                "speech_tts_synthesize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "text", "Hello world" },
                    { "outputAudio", outputFile },
                    { "language", language },
                    { "voice", voice }
                });

            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);

            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;
            Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

            // Verify voice was used
            Assert.True(resultProperty.TryGetProperty("voice", out var voiceProperty));
            Assert.Equal(voice, voiceProperty.GetString());

            // Verify language
            Assert.True(resultProperty.TryGetProperty("language", out var languageProperty));
            Assert.Equal(language, languageProperty.GetString());

            // Verify file exists
            Assert.True(File.Exists(outputFile));
        }
        finally
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Theory]
    [InlineData("Riff8Khz16BitMonoPcm")]
    [InlineData("Riff24Khz16BitMonoPcm")]
    [InlineData("Audio16Khz32KBitRateMonoMp3")]
    public async Task Should_synthesize_speech_with_different_formats(string format)
    {
        // Test TTS synthesis with different audio formats
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var extension = format.Contains("Mp3") ? ".mp3" : ".wav";
        var outputFile = Path.Combine(Path.GetTempPath(), $"tts-test-{format}-{Guid.NewGuid()}{extension}");

        try
        {
            var result = await CallToolAsync(
                "speech_tts_synthesize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "text", "Testing different audio formats" },
                    { "outputAudio", outputFile },
                    { "language", "en-US" },
                    { "format", format }
                });

            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);

            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;
            Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

            // Verify format
            Assert.True(resultProperty.TryGetProperty("format", out var formatProperty));
            Assert.Equal(format, formatProperty.GetString());

            // Verify file exists and has content
            Assert.True(File.Exists(outputFile));
            var fileInfo = new FileInfo(outputFile);
            Assert.True(fileInfo.Length > 0);
        }
        finally
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task Should_handle_invalid_text_input()
    {
        // Test error handling for empty text
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var outputFile = Path.Combine(Path.GetTempPath(), $"tts-test-invalid-{Guid.NewGuid()}.wav");

        try
        {
            var result = await CallToolAsync(
                "speech_tts_synthesize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "text", "" }, // Empty text should fail validation
                    { "outputAudio", outputFile },
                    { "language", "en-US" }
                });

            // Should return error response
            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task Should_handle_invalid_language_format()
    {
        // Test error handling for invalid language format
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var outputFile = Path.Combine(Path.GetTempPath(), $"tts-test-invalid-lang-{Guid.NewGuid()}.wav");

        try
        {
            var result = await CallToolAsync(
                "speech_tts_synthesize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "text", "Hello world" },
                    { "outputAudio", outputFile },
                    { "language", "invalid-format" } // Invalid language format
                });

            // Should return error response
            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
    }

    [Fact]
    public async Task Should_handle_large_text_input()
    {
        // Test TTS with larger text to verify streaming works correctly
        var aiServicesEndpoint = $"https://{Settings.ResourceBaseName}.cognitiveservices.azure.com/";
        var outputFile = Path.Combine(Path.GetTempPath(), $"tts-test-large-{Guid.NewGuid()}.wav");

        // Create a longer text (around 1000 words)
        var largeText = string.Join(" ", Enumerable.Repeat(
            "This is a test of text to speech synthesis with a longer input to verify that streaming works correctly.",
            50));

        try
        {
            var result = await CallToolAsync(
                "speech_tts_synthesize",
                new()
                {
                    { "subscription", Settings.SubscriptionId },
                    { "endpoint", aiServicesEndpoint },
                    { "text", largeText },
                    { "outputAudio", outputFile },
                    { "language", "en-US" }
                });

            Assert.NotNull(result);
            var resultText = result.ToString();
            Assert.NotNull(resultText);

            var jsonResult = JsonDocument.Parse(resultText);
            var resultObject = jsonResult.RootElement;
            Assert.True(resultObject.TryGetProperty("result", out var resultProperty));

            // Verify file exists and is significantly larger than a short phrase
            Assert.True(File.Exists(outputFile));
            var fileInfo = new FileInfo(outputFile);
            Assert.True(fileInfo.Length > 50000, "Large text should produce a substantial audio file");
        }
        finally
        {
            if (File.Exists(outputFile))
            {
                File.Delete(outputFile);
            }
        }
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

