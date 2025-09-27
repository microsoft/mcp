// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Areas;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Tools.Speech.Commands.Stt;
using Azure.Mcp.Tools.Speech.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Mcp.Tools.Speech;

public class SpeechSetup : IAreaSetup
{
    public string Name => "speech";

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISpeechService, SpeechService>();
        services.AddSingleton<SttRecognizeCommand>();
    }

    public CommandGroup RegisterCommands(IServiceProvider serviceProvider)
    {
        var speech = new CommandGroup(Name,
            """
            Speech operations - Commands for Azure AI Services Speech functionality including speech-to-text (STT) 
            recognition, audio processing, and language detection. Use this tool when you need to convert spoken 
            audio to text, process audio files, or work with speech recognition services. This tool supports 
            multiple audio formats, configurable recognition languages, profanity filtering options, and both 
            simple and detailed output formats. This tool is a hierarchical MCP command router where sub-commands 
            are routed to MCP servers that require specific fields inside the "parameters" object. To invoke a 
            command, set "command" and wrap its arguments in "parameters". Set "learn=true" to discover available 
            sub-commands for different Azure AI Services Speech operations. Note that this tool requires Azure AI 
            Services Speech endpoints and will only access speech resources accessible to the authenticated user.
            """);

        var stt = new CommandGroup(
            name: "stt",
            description: "Speech-to-text operations - Commands for converting spoken audio to text using Azure AI Services Speech recognition.");

        var sttRecognize = serviceProvider.GetRequiredService<SttRecognizeCommand>();
        stt.AddCommand(sttRecognize.Name, sttRecognize);

        speech.AddSubGroup(stt);
        return speech;
    }
}
