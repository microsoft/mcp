// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Options.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.PublicApis;

public sealed class GetWorkloadApisCommand(ILogger<GetWorkloadApisCommand> logger) : GlobalCommand<WorkloadCommandOptions>()
{
    private readonly ILogger<GetWorkloadApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            if (options.WorkloadType!.Equals("common", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = "No workload of type 'common' exists. Did you mean 'platform'?. A full list of supported workloads can be found using the list_workloads command";
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var apis = await fabricService.GetWorkloadPublicApis(options.WorkloadType, cancellationToken);

            context.Response.Results = ResponseResult.Create(apis, FabricJsonContext.Default.FabricWorkloadPublicApi);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error getting Fabric public APIs for workload {}", options.WorkloadType);
            if (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = $"No workload of type '{options.WorkloadType}' exists. A full list of supported workloads can be found using the list_workloads command";
            }
            else
            {
                context.Response.Status = httpEx.StatusCode ?? HttpStatusCode.InternalServerError;
                context.Response.Message = httpEx.Message;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Fabric public APIs for workload {}", options.WorkloadType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
