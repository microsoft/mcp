// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Options.BestPractices;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;

public sealed class GetBestPracticeCommand(ILogger<GetBestPracticeCommand> logger) : GlobalCommand<GetBestPracticesOptions>()
{
    private const string CommandTitle = "Get API Examples";

    private readonly ILogger<GetBestPracticeCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Option<string> _topicOption = FabricOptionDefinitions.Topic;

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves all best practice resources on a specific topic.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_topicOption);
    }

    protected override GetBestPracticesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Topic = parseResult.GetValueForOption(_topicOption);
        return options;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return Task.FromResult(context.Response);
            }

            if (string.IsNullOrEmpty(options.Topic))
            {
                context.Response.Status = 400;
                context.Response.Message = "Topic is required";
                return Task.FromResult(context.Response);
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var bestPractices = fabricService.GetTopicBestPractices(options.Topic);

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
