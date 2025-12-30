// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.AutoexportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.AutoexportJob;

public sealed class AutoexportJobGetCommand(ILogger<AutoexportJobGetCommand> logger)
    : BaseManagedLustreCommand<AutoexportJobGetOptions>(logger)
{
    private const string CommandTitle = "Get Azure Managed Lustre Autoexport Job";

    private new readonly ILogger<AutoexportJobGetCommand> _logger = logger;

    public override string Id => "9a3b7e2f-4d6c-8a1e-b5f3-2c7d8e9a1b4f";

    public override string Name => "get";

    public override string Description =>
        """
        Gets the details of auto export jobs for an Azure Managed Lustre filesystem. Use this to retrieve the status, configuration, and progress information of autoexport operations that sync data from the Lustre filesystem to the linked blob storage container. If job-name is provided, returns details of a specific job; otherwise returns all jobs for the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        - resource-group: The resource group containing the filesystem
        - subscription: The subscription containing the filesystem
        Optional options:
        - job-name: The name of a specific autoexport job (if omitted, all jobs are returned)
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

        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption);
        command.Options.Add(ManagedLustreOptionDefinitions.JobNameOption.AsOptional());
    }

    protected override AutoexportJobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName ??= parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.JobNameOption.Name);
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
            var svc = context.GetService<IManagedLustreService>();

            if (!string.IsNullOrWhiteSpace(options.JobName))
            {
                // Get specific job
                var result = await svc.GetAutoexportJobAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileSystemName!,
                    options.JobName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new AutoexportJobGetResult(result), ManagedLustreJsonContext.Default.AutoexportJobGetResult);
            }
            else
            {
                // List all jobs
                var results = await svc.ListAutoexportJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileSystemName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new AutoexportJobListResult(results ?? []), ManagedLustreJsonContext.Default.AutoexportJobListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autoexport job {JobName} for AMLFS filesystem {FileSystemName}. Options: {@Options}", options.JobName, options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record AutoexportJobGetResult(Models.AutoexportJob Job);
    public record AutoexportJobListResult(List<Models.AutoexportJob> Jobs);
}
