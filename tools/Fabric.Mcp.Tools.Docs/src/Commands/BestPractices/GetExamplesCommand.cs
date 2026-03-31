// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Options.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.BestPractices;

public sealed class GetExamplesCommand(ILogger<GetExamplesCommand> logger) : GlobalCommand<WorkloadCommandOptions>()
{

    private readonly ILogger<GetExamplesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(FabricOptionDefinitions.WorkloadType);
    }

    protected override WorkloadCommandOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkloadType = parseResult.GetValueOrDefault<string>(FabricOptionDefinitions.WorkloadType.Name);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var fabricService = context.GetService<IFabricPublicApiService>();
            var availableExamples = await fabricService.GetWorkloadExamplesAsync(options.WorkloadType!, cancellationToken);

            context.Response.Results = ResponseResult.Create(new(availableExamples), FabricJsonContext.Default.ExampleFileResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting examples for workload {}", options.WorkloadType);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ExampleFileResult(IDictionary<string, string> Examples);
}
