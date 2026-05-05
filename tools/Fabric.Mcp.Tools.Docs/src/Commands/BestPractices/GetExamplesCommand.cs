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

[CommandMetadata(
    Id = "3efdeea3-ee84-43e7-b7a9-c4accb03795a",
    Name = "api-examples",
    Title = "API Examples",
    Description = "Retrieves example API request and response files for a Fabric workload. Use this when the user needs sample API calls or implementation examples. Returns dictionary of example files with their contents.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetExamplesCommand(IFabricPublicApiService service, ILogger<GetExamplesCommand> logger)
    : GlobalCommand<WorkloadCommandOptions>()
{
    private readonly ILogger<GetExamplesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IFabricPublicApiService _service = service ?? throw new ArgumentNullException(nameof(service));

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
            var availableExamples = await _service.GetWorkloadExamplesAsync(options.WorkloadType!, cancellationToken);

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
