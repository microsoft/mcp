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

[CommandMetadata(
    Id = "0a73ecc9-d257-4ff3-8e05-fd3158c2cd31",
    Name = "best-practices",
    Title = "Best Practices",
    Description = "Retrieves embedded best practice documentation for a specific Fabric topic. Use this when the user needs guidance, recommendations, or implementation patterns for Fabric features. Returns detailed best practice content.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetBestPracticesCommand(IFabricPublicApiService service, ILogger<GetBestPracticesCommand> logger)
    : GlobalCommand<GetBestPracticesOptions>()
{
    private readonly ILogger<GetBestPracticesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

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
            var bestPractices = _service.GetTopicBestPractices(options.Topic!);

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
