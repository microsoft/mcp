// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Core.Commands;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.ManagedLustre.Options;
using Azure.Mcp.Tools.ManagedLustre.Options.FileSystem.ImportJob;
using Azure.Mcp.Tools.ManagedLustre.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.ManagedLustre.Commands.FileSystem.ImportJob;

public sealed class ImportJobGetCommand(ILogger<ImportJobGetCommand> logger)
    : BaseManagedLustreCommand<ImportJobGetOptions>(logger)
{
    private const string CommandTitle = "Get Azure Managed Lustre Import Job";

    private new readonly ILogger<ImportJobGetCommand> _logger = logger;

    public override string Id => "c2g4d6f8-0e3a-5c7d-9f1b-3e5a7c9f1d3f";

    public override string Name => "get";

    public override string Description =>
        """
        Gets import job details or lists all import jobs for an Azure Managed Lustre filesystem. If job-name is provided, returns details for that specific job. If job-name is omitted, returns a list of all import jobs for the filesystem.
        Required options:
        - filesystem-name: The name of the AMLFS filesystem
        Optional options:
        - job-name: Name of specific import job to get (omit to list all jobs)
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
        command.Options.Add(ManagedLustreOptionDefinitions.FileSystemNameOption.AsRequired());
        command.Options.Add(ManagedLustreOptionDefinitions.OptionalJobNameOption.AsOptional());
    }

    protected override ImportJobGetOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup ??= parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name);
        options.FileSystemName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.FileSystemNameOption.Name);
        options.JobName = parseResult.GetValueOrDefault<string>(ManagedLustreOptionDefinitions.OptionalJobNameOption.Name);
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
                var result = await svc.GetImportJobAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileSystemName!,
                    options.JobName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(result), ManagedLustreJsonContext.Default.ImportJobGetResult);
            }
            else
            {
                // List all jobs
                var results = await svc.ListImportJobsAsync(
                    options.Subscription!,
                    options.ResourceGroup!,
                    options.FileSystemName!,
                    options.Tenant,
                    options.RetryPolicy,
                    cancellationToken);

                context.Response.Results = ResponseResult.Create(new(results), ManagedLustreJsonContext.Default.ImportJobListResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting import job(s) for AMLFS filesystem {FileSystem}. Options: {@Options}",
                options.FileSystemName, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    public record ImportJobGetResult(Models.ImportJob Job);
    public record ImportJobListResult(List<Models.ImportJob> Jobs);
}
