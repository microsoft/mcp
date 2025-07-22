
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Options.Guidance;
using AzureMcp.Commands;
using Microsoft.Extensions.Logging;
using System.CommandLine.Parsing;
using System.Threading;

using System.Threading.Tasks;
namespace AzureMcp.Areas.Startups.Commands.Guidance;
public sealed class StartupsGuidanceCommand(ILogger<StartupsGuidanceCommand> logger) : GlobalCommand<StartupsGuidanceOptions>()
{
    private const string GuidanceCommandTitle = "Get Guidance from Microsoft for Startups";
    private readonly ILogger<StartupsGuidanceCommand> _logger = logger;

    private const string GuidanceCommandDescription = "Receive program guidance for building with Microsoft for Startups.";
    private const string GuidanceCommandName = "get";
    public override string Name => GuidanceCommandName;
    public override string Title => GuidanceCommandTitle;
    public override string Description => GuidanceCommandDescription;
    [McpServerTool(Destructive = false, ReadOnly = true, Title = GuidanceCommandTitle)]
    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var info = new GuidanceInfo(
            Title: GuidanceCommandTitle,
            Description: GuidanceCommandDescription,
            Link: "https://startups.microsoft.com/"
        );
    var result = new GuidanceCommandResult(info);
    context.Response.Results = ResponseResult.Create(
        result, GuidanceJsonContext.Default.GuidanceCommandResult);
    return Task.FromResult(context.Response);
    }
    public record GuidanceInfo(string Title, string Description, string Link);
    public record GuidanceCommandResult(GuidanceInfo Info){}

}

// System.Text.Json serialization context for GuidanceCommandResult
[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand.GuidanceCommandResult))]
internal partial class GuidanceJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
