// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.PublicApi.Options;
using Fabric.Mcp.Tools.PublicApi.Options.PublicApis;
using Fabric.Mcp.Tools.PublicApi.Services;
using Microsoft.Extensions.Logging;

namespace Fabric.Mcp.Tools.PublicApi.Commands.PublicApis;

public sealed class GetWorkloadApisCommand(ILogger<GetWorkloadApisCommand> logger) : GlobalCommand<WorkloadCommandOptions>()
{
    private const string CommandTitle = "Get Workload API Specification";
    private readonly ILogger<GetWorkloadApisCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Name => "get";

    public override string Description =>
        """
        Retrieve the complete OpenAPI/Swagger specification for a specific Microsoft Fabric workload.
        Requires the workload type (e.g., 'notebook', 'report'). Returns the full API specification
        in JSON format along with any supplementary definition files. Use 'discover-workloads'
        command first to see available workload types.
        """;

    public override string Title => CommandTitle;

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = false,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

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

    public override async Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult)
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
                context.Response.Status = 404;
                context.Response.Message = "No workload of type 'common' exists. Did you mean 'platform'?. A full list of supported workloads can be found using the discover-workloads command";
                return context.Response;
            }

            var fabricService = context.GetService<IFabricPublicApiService>();
            var apis = await fabricService.GetWorkloadPublicApis(options.WorkloadType);

            context.Response.Results = ResponseResult.Create(apis, FabricJsonContext.Default.FabricWorkloadPublicApi);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP error getting Fabric public APIs for workload {}", options.WorkloadType);
            if (httpEx.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                context.Response.Status = 404;
                context.Response.Message = $"No workload of type '{options.WorkloadType}' exists. A full list of supported workloads can be found using the discover-workloads command";
            }
            else
            {
                context.Response.Status = (int)(httpEx.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
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
