// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Commands.Tts;
using Azure.Mcp.Tools.Speech.Services;
using Azure.Mcp.Tools.Speech.Services.Recognizers;
using Azure.Mcp.Tools.Speech.Services.Synthesizers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;

namespace Azure.Mcp.Tools.Speech;

public class SpeechSetup : IAreaSetup
{
    public string Name => "speech";

    public string Title => "Azure AI Speech";

    public void ConfigureServices(IServiceCollection services)
    {
        // New recognizer-based architecture for STT
        services.AddSingleton<IFastTranscriptionRecognizer, FastTranscriptionRecognizer>();
        services.AddSingleton<IRealtimeTranscriptionRecognizer, RealtimeTranscriptionRecognizer>();

        // New synthesizer-based architecture for TTS
        services.AddSingleton<IRealtimeTtsSynthesizer, RealtimeTtsSynthesizer>();

        // Orchestration service
        services.AddSingleton<ISpeechService, SpeechService>();

        // Commands
        services.AddSingleton<SttRecognizeCommand>();
        services.AddSingleton<TtsSynthesizeCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var speech = new CommandGroup(Name,
            """
            Speech operations – Commands to work with Azure AI Services Speech,
            including speech-to-text (STT) recognition, text-to-speech (TTS)
            synthesis, audio processing, and language detection. Uses a hierarchical
            MCP command model with command and parameters; set learn=true to
            discover sub-commands. Supports multiple audio formats, languages, and
            output options.
            """, Title);

        var stt = new CommandGroup(
            name: "stt",
            description: """
            Speech-to-text operations - Commands for converting spoken audio to text 
            using Azure AI Services Speech recognition.
            """);

        var sttRecognize = serviceProvider.GetRequiredService<SttRecognizeCommand>();
        stt.AddCommand(sttRecognize.Name, sttRecognize);

        speech.AddSubGroup(stt);

        var tts = new CommandGroup(
            name: "tts",
            description: """
            Text-to-speech operations - Commands for converting text to spoken audio 
            using Azure AI Services Speech synthesis.
            """);

        var ttsSynthesize = serviceProvider.GetRequiredService<TtsSynthesizeCommand>();
        tts.AddCommand(ttsSynthesize.Name, ttsSynthesize);

        speech.AddSubGroup(tts);

        return speech;
    }
}
