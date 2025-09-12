// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Options.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.BestPractices;

public sealed class GetWorkloadDefinitionCommand(ILogger<GetWorkloadDefinitionCommand> logger) : GlobalCommand<WorkloadCommandOptions>()
{
    private const string CommandTitle = "Get Workload Item Definition";

    private readonly ILogger<GetWorkloadDefinitionCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Option<string> _workloadTypeOption = FabricOptionDefinitions.WorkloadType;

    public override string Name => "get";

    public override string Description =>
        """
        Retrieve the JSON schema definitions for specific items within a Microsoft Fabric workload's API.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new() { Destructive = false, ReadOnly = true };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(_workloadTypeOption);
    }

    protected override WorkloadCommandOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.WorkloadType = parseResult.GetValueOrDefault(_workloadTypeOption);
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
            var workloadItemDefinition = fabricService.GetWorkloadItemDefinition(options.WorkloadType!);

            context.Response.Results = ResponseResult.Create(workloadItemDefinition, FabricJsonContext.Default.String);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx, "Invalid argument for workload {}", options.WorkloadType);
            context.Response.Status = 404;
            context.Response.Message = $"No item definition found for workload {options.WorkloadType}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting examples for workload {}", options.WorkloadType);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
