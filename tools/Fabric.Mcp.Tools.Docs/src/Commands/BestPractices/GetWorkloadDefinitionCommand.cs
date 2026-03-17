// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Fabric.Mcp.Tools.Docs.Options;
using Fabric.Mcp.Tools.Docs.Options.PublicApis;
using Fabric.Mcp.Tools.Docs.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Fabric.Mcp.Tools.Docs.Commands.BestPractices;

public sealed class GetWorkloadDefinitionCommand(ILogger<GetWorkloadDefinitionCommand> logger) : GlobalCommand<WorkloadCommandOptions>()
{
    private const string CommandTitle = "Item Definitions";

    private readonly ILogger<GetWorkloadDefinitionCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public override string Id => "445c49f3-2a5d-478a-82ca-87fde1a7943e";

    public override string Name => "item_definitions";

    public override string Description =>
        "Retrieves JSON schema definitions for items in a Fabric workload API. Use this when the user needs to understand item structure or validate item definitions. Returns schema definitions for the specified workload.";

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

    public override Task<CommandResponse> ExecuteAsync(CommandContext context, ParseResult parseResult, CancellationToken cancellationToken)
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
