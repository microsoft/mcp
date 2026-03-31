// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Options.BestPractices;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.BestPractices;

public sealed class GetBestPracticesCommand(ILogger<GetBestPracticesCommand> logger) : GlobalCommand<GetBestPracticesOptions>()
{

    private readonly ILogger<GetBestPracticesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.Topic);
    }

    protected override GetBestPracticesOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Topic = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.Topic.Name);
        return options;
    }

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
            context.Response.Status = HttpStatusCode.NotFound;
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
