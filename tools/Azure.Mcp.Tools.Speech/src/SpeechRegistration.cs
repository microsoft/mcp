// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Commands.Tts;
using Azure.Mcp.Tools.Speech.Services;
using Azure.Mcp.Tools.Speech.Services.Recognizers;
using Azure.Mcp.Tools.Speech.Services.Synthesizers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Mcp.Core.Areas;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Commands.Descriptors;

namespace Azure.Mcp.Tools.Speech;

public sealed class SpeechRegistration : IAreaRegistration
{
    public static string AreaName => "speech";

    public static string AreaTitle => "Azure AI Speech";

    public static CommandCategory Category => CommandCategory.AzureServices;

    public static CommandGroupDescriptor GetCommandDescriptors() => new()
    {
        Name = AreaName,
        Description = "Azure AI Speech operations.",
        Title = AreaTitle,
        SubGroups =
        [
            new CommandGroupDescriptor
            {
                Name = "stt",
                Description = "stt operations.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "c725eb52-ca2c-4fe4-9422-935e7557b701",
                        Name = "recognize",
                        Description = "Recognize speech from an audio file using Azure AI Services Speech. This command takes an audio file and converts it to text using advanced speech recognition capabilities. You must provide an Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/) and a path to the audio file. Supported audio formats include WAV, MP3, OPUS/OGG, FLAC, ALAW, MULAW, MP4, M4A, and AAC. Compressed formats require GStreamer to be installed on the system. Optional parameters include language specification, phrase hints for better accuracy, output format (simple or detailed), and profanity filtering.",
                        Title = "Recognize",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = true,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "endpoint",
                                Description = "The Azure AI Services endpoint URL (e.g., https://your-service.cognitiveservices.azure.com/).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "file",
                                Description = "Path to the audio file to recognize.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "language",
                                Description = "The language for speech recognition (e.g., en-US, es-ES). Default is en-US.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "phrases",
                                Description = "Phrase hints to improve recognition accuracy. Can be specified multiple times (--phrases \"phrase1\" --phrases \"phrase2\") or as comma-separated values (--phrases \"phrase1,phrase2\").",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "format",
                                Description = "Output format: simple or detailed.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "profanity",
                                Description = "Profanity filter: masked, removed, or raw. Default is masked.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(SttRecognizeCommand)
                    },
                ],
            },
            new CommandGroupDescriptor
            {
                Name = "tts",
                Description = "tts operations.",
                Commands =
                [
                    new CommandDescriptor
                    {
                        Id = "d6f6687f-feee-4e15-9b98-71aea4076e04",
                        Name = "synthesize",
                        Description = "Convert text to speech using Azure AI Services Speech. This command takes text input and generates an audio file using advanced neural text-to-speech capabilities. You must provide an Azure AI Services endpoint (e.g., https://your-service.cognitiveservices.azure.com/), the text to convert, and an output file path. Optional parameters include language specification (default: en-US), voice selection, audio output format (default: Riff24Khz16BitMonoPcm), and custom voice endpoint ID. The command supports a wide variety of output formats and neural voices for natural-sounding speech synthesis.",
                        Title = "Synthesize",
                        Annotations = new ToolAnnotations
                        {
                            Destructive = false,
                            Idempotent = true,
                            OpenWorld = false,
                            ReadOnly = false,
                            LocalRequired = true,
                            Secret = false,
                        },
                        Options =
                        [
                            new OptionDescriptor
                            {
                                Name = "endpoint",
                                Description = "The Azure AI Services endpoint URL (e.g., https://your-service.cognitiveservices.azure.com/).",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "text",
                                Description = "The text to convert to speech.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "outputAudio",
                                Description = "Path where the synthesized audio file will be saved.",
                                TypeName = "string",
                                Required = true,
                            },
                            new OptionDescriptor
                            {
                                Name = "language",
                                Description = "The language for speech recognition (e.g., en-US, es-ES). Default is en-US.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "voice",
                                Description = "The voice to use for speech synthesis (e.g., en-US-JennyNeural). If not specified, the default voice for the language will be used.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "format",
                                Description = "Output format: simple or detailed.",
                                TypeName = "string",
                            },
                            new OptionDescriptor
                            {
                                Name = "endpointId",
                                Description = "The endpoint ID of a custom voice model for speech synthesis.",
                                TypeName = "string",
                            },
                        ],
                        Kind = CommandKind.Subscription,
                        HandlerType = nameof(TtsSynthesizeCommand)
                    },
                ],
            },
        ],
    };

    public static void RegisterServices(IServiceCollection services)
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

    public static IBaseCommand ResolveHandler(string handlerTypeName, IServiceProvider serviceProvider) =>
        handlerTypeName switch
        {
            nameof(SttRecognizeCommand) => serviceProvider.GetRequiredService<SttRecognizeCommand>(),
            nameof(TtsSynthesizeCommand) => serviceProvider.GetRequiredService<TtsSynthesizeCommand>(),
            _ => throw new InvalidOperationException($"Unknown handler type '{{handlerTypeName}}' in speech area.")
        };
}
