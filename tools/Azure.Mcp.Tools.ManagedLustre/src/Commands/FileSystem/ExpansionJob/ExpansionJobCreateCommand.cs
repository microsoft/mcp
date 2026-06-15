// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ExpansionJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Extensions;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ExpansionJob;

[CommandMetadata(
    Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    Name = "create",
    Title = "Create Azure Managed Lustre Expansion Job",
    Description = """
        Creates an expansion job to increase the storage capacity of an Azure Managed Lustre (AMLFS) file system. The expansion job resizes the filesystem to the specified new capacity in TiB. The new size must be a multiple of the SKU step size and greater than the current storage capacity.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - new-size: The new storage capacity in TiB after expansion
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        """,
    Destructive = true,
    Idempotent = false,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = false)]
public sealed class ExpansionJobCreateCommand(IManagedLustreService service, ILogger<ExpansionJobCreateCommand> logger)
    : BaseManagedLustreCommand<ExpansionJobCreateOptions>(logger)
{

    private readonly IManagedLustreService _service = service;
    private new readonly ILogger<ExpansionJobCreateCommand> _logger = logger;

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.NewSizeOption);
        command.Options.Add(ManagedLustreOptionDefinitions.ExpansionJobNameOption);
    }

    protected override ExpansionJobCreateOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.NewSizeTiB = parseResult.GetValueOrDefault<float>(ManagedLustreOptionDefinitions.NewSizeOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.ExpansionJobNameOption.Name);
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
            var jobName = await _service.CreateExpansionJobAsync(
                options.Subscription!,
                options.ResourceGroup!,
                options.FileSystemName!,
                options.NewSizeTiB!.Value,
                options.JobName,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken);

            context.Response.Results = ResponseResult.Create(new(jobName), ManagedLustreJsonContext.Default.ExpansionJobCreateResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expansion job for AMLFS filesystem {FileSystem}.",
                options.FileSystemName);
            HandleException(context, ex);
        }

        return context.Response;
    }

    internal record ExpansionJobCreateResult(string JobName);
}
