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

[CommandMetadata(
    Id = "97229a98-c1ae-4255-a6e2-07631c2a42c5",
    Name = "workload-api-spec",
    Title = "Workload API Specification",
    Description = "Retrieves the complete OpenAPI specification for a specific Fabric workload. Use this when the user needs detailed API documentation for a workload like notebooks or reports. Returns full API spec in JSON format.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetWorkloadApisCommand(IFabricPublicApiService service, ILogger<GetWorkloadApisCommand> logger)
    : GlobalCommand<WorkloadCommandOptions>()
{
    private readonly ILogger<GetWorkloadApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            if (options.WorkloadType!.Equals("common", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Status = HttpStatusCode.NotFound;
                context.Response.Message = "No workload of type 'common' exists. Did you mean 'platform'?. A full list of supported workloads can be found using the list_workloads command";
                return context.Response;
            }

            var apis = await _service.GetWorkloadPublicApis(options.WorkloadType, cancellationToken);

            context.Response.Results = ResponseResult.Create(apis, FabricJsonContext.Default.FabricWorkloadPublicApi);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error getting Fabric public APIs for workload {}", options.WorkloadType);
            if (httpEx.StatusCode == HttpStatusCode.NotFound)
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
