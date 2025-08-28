// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Options.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;

public sealed class GetWorkloadApisCommand(ILogger<GetWorkloadApisCommand> logger) : GlobalCommand<GetWorkloadApisOptions>()
{
    private const string CommandTitle = "Get Fabric Item Details";
    private readonly ILogger<GetWorkloadApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly Option<string> _workloadTypeOption = FabricOptionDefinitions.WorkloadType;

    public override string Name => "details";

    public override string Description =>
        """
        Get open API swagger specifications for the Fabric public APIs of the specified workload.
        Supported workloads are can be found by calling this command with workload type "all".
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
                context.Response.Message = "Workspace ID is required";
                return context.Response;
            }

            if (options.WorkloadType.Equals("common", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Status = 404;
                context.Response.Message = "No workload of type 'common' exists. Did you mean 'platform'?. A list of all available workloads can be found be calling this command with workload type 'all'.";
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var apis = await fabricService.ListFabricWorkloadPublicApis(options.WorkloadType);

            context.Response.Results = ResponseResult.Create(apis, FabricJsonContext.Default.FabricWorkloadPublicApi);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Fabric public APIs for workload {}", options.WorkloadType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
