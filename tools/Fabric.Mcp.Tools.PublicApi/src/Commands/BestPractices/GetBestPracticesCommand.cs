// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Options.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;

public sealed class GetBestPracticesCommand(ILogger<GetBestPracticesCommand> logger) : GlobalCommand<GetBestPracticesOptions>()
{
    private const string CommandTitle = "Get API Examples";

    private readonly ILogger<GetBestPracticesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Option<string> _topicOption = FabricOptionDefinitions.Topic;

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves embedded best practice documentation and guidance for a specific Microsoft Fabric topic.
        Use this command when you need detailed recommendations, guidelines, or best practices for 
        implementing or working with specific Fabric features, APIs, or development patterns.
        The topic parameter should match available embedded resource names for Fabric best practices.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_topicOption);
    }

    protected override GetBestPracticesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Topic = parseResult.GetValueOrDefault(_topicOption);
        return options;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var fabricService = context.GetService<IFabricPublicApiService>();
            var bestPractices = fabricService.GetTopicBestPractices(options.Topic!);

            context.Response.Results = ResponseResult.Create(bestPractices, FabricJsonContext.Default.IEnumerableString);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx, "No best practice resources found for {}", options.Topic);
            context.Response.Status = 404;
            context.Response.Message = $"No best practice resources found for {options.Topic}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best practices for topic {}", options.Topic);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
