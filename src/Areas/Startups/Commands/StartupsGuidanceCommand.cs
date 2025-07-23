
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using AzureMcp.Areas.Startups.Options.Guidance;
using AzureMcp.Commands;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using AzureMcp.Areas.LoadTesting.Options;
using AzureMcp.Models.Option;
using AzureMcp.Commands.Subscription;
using AzureMcp.Areas.Startups.Options;

namespace AzureMcp.Areas.Startups.Commands.Guidance;
public sealed class StartupsGuidanceCommand<[DynamicallyAccessedMembers(TrimAnnotations.CommandAnnotations)] TOptions>(ILogger<StartupsGuidanceCommand<TOptions>> logger)
    : SubscriptionCommand<TOptions> where TOptions : AzureMcp.Options.SubscriptionOptions, StartupsGuidanceOptions, new()
{
    private const string CommandTitle = "Get Guidance from Microsoft for Startups";
    private readonly ILogger<StartupsGuidanceCommand<TOptions>> _logger = logger;
    public override string Name => "get";
    public override string Description => "Receive program guidance for building with Microsoft for Startups.";
    public override string Title => CommandTitle;

    [McpServerTool(Destructive = false, ReadOnly = true, Title = CommandTitle)]
    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var info = new StartupsGuidanceInfo(
            Title: CommandTitle,
            Description: Description,
            Link: "https://startups.microsoft.com/"
        );
        context.Response.Results = ResponseResult.Create<StartupsGuidanceCommandResult>(
            new StartupsGuidanceCommandResult(info),
            GuidanceJsonContext.Default.StartupsGuidanceCommandResultBaseLoadTestingOptions);
        return Task.FromResult(context.Response);
    }
    public record StartupsGuidanceInfo(string Title, string Description, string Link);
    public record StartupsGuidanceCommandResult(StartupsGuidanceInfo Info);
}

[System.Text.Json.Serialization.JsonSourceGenerationOptions(WriteIndented = false, GenerationMode = System.Text.Json.Serialization.JsonSourceGenerationMode.Default)]
[System.Text.Json.Serialization.JsonSerializable(typeof(StartupsGuidanceCommand<BaseLoadTestingOptions>.StartupsGuidanceCommandResult))]
internal partial class GuidanceJsonContext : System.Text.Json.Serialization.JsonSerializerContext;
