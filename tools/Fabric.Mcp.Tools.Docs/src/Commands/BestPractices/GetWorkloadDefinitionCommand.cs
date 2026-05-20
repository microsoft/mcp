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

namespace Fabric.Mcp.Tools.Docs.Commands.BestPractices;

[CommandMetadata(
    Id = "445c49f3-2a5d-478a-82ca-87fde1a7943e",
    Name = "item-definitions",
    Title = "Item Definitions",
    Description = "Retrieves JSON schema definitions for items in a Fabric workload API. Use this when the user needs to understand item structure or validate item definitions. Returns schema definitions for the specified workload.",
    Destructive = false,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = true,
    LocalRequired = false,
    Secret = false)]
public sealed class GetWorkloadDefinitionCommand(IFabricPublicApiService service, ILogger<GetWorkloadDefinitionCommand> logger)
    : GlobalCommand<WorkloadCommandOptions>()
{
    private readonly ILogger<GetWorkloadDefinitionCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return Task.FromResult(context.Response);
        }

        var options = BindOptions(parseResult);

        try
        {
            var workloadItemDefinition = _service.GetWorkloadItemDefinition(options.WorkloadType!);

            context.Response.Results = ResponseResult.Create(workloadItemDefinition, FabricJsonContext.Default.String);
        }
        catch (ArgumentException argEx)
        {
            _logger.LogError(argEx, "Invalid argument for workload {}", options.WorkloadType);
            context.Response.Status = HttpStatusCode.NotFound;
            context.Response.Message = $"No item definition found for workload {options.WorkloadType}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item definition for workload {}", options.WorkloadType);
            HandleException(context, ex);
        }

        return Task.FromResult(context.Response);
    }
}
