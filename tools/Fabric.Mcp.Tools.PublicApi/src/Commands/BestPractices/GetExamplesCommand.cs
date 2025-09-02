// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Options.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;

public sealed class GetExamplesCommand(ILogger<GetExamplesCommand> logger) : GlobalCommand<GetWorkloadApisOptions>()
{
    private const string CommandTitle = "Get API Examples";

    private readonly ILogger<GetExamplesCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Option<string> _workloadTypeOption = FabricOptionDefinitions.WorkloadType;

    public override string Name => "get";

    public override string Description =>
        """
        Retrieve all example API request/response files for a specific Microsoft Fabric workload.
        Requires the workload type (e.g., 'notebook', 'report'). Returns a dictionary mapping 
        file paths to their contents, containing sample API calls, responses, and implementation 
        examples to help with API integration and development.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.AddOption(_workloadTypeOption);
    }

    protected override GetWorkloadApisOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkloadType = parseResult.GetValueForOption(_workloadTypeOption);
        return options;
    }

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
    {
        var options = BindOptions(parseResult);

        try
        {
            if (!Validate(parseResult.CommandResult, context.Response).IsValid)
            {
                return context.Response;
            }

            if (string.IsNullOrEmpty(options.WorkloadType))
            {
                context.Response.Status = 400;
                context.Response.Message = "Workload type is required";
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var availableExamples = await fabricService.GetExamplesAsync(options.WorkloadType);

            context.Response.Results = ResponseResult.Create(new ExampleFileResult(availableExamples), FabricJsonContext.Default.ExampleFileResult);
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
