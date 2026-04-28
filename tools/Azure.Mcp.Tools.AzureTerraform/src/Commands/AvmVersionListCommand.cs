// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Models;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

public sealed class AvmVersionListCommand(
    ILogger<AvmVersionListCommand> logger,
    IAvmDocsService avmDocsService) : BaseCommand<AvmVersionOptions>
{
    private readonly ILogger<AvmVersionListCommand> _logger = logger;
    private readonly IAvmDocsService _avmDocsService = avmDocsService;

    public override string Id => "d4e5f6a7-b8c9-0123-def0-345678901bcd";

    public override string Name => "versions";

    public override string Description =>
        """
        Retrieves all available versions of a specified Azure Verified Module (AVM).
        Returns version tags with creation dates, sorted from newest to oldest.
        The first version in the list is the latest. Use --module-name to specify
        the module (e.g., avm-res-storage-storageaccount).
        """;

    public override string Title => "List AVM Module Versions";

    public override ToolMetadata Metadata => new()
    {
        Destructive = false,
        Idempotent = true,
        OpenWorld = true,
        ReadOnly = true,
        LocalRequired = false,
        Secret = false
    };

    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(AzureTerraformOptionDefinitions.AvmModuleName.AsRequired());
    }

    protected override AvmVersionOptions BindOptions(ParseResult parseResult)
    {
        return new AvmVersionOptions
        {
            ModuleName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.AvmModuleName.Name)
        };
    }

    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        if (!Validate(parseResult.CommandResult, context.Response).IsValid)
        {
            return context.Response;
        }

        var options = BindOptions(parseResult);

        try
        {
            var versions = await _avmDocsService.GetVersionsAsync(
                options.ModuleName!,
                cancellationToken).ConfigureAwait(false);

            var result = new Models.AvmVersionListResult
            {
                ModuleName = options.ModuleName!,
                Versions = versions
            };

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AvmVersionListResult);
            context.Response.Message = string.Empty;

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "avm")
                .AddTag(AzureTerraformTelemetryTags.ModuleName, options.ModuleName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving versions for AVM module {ModuleName}", options.ModuleName);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
