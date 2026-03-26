// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

public sealed class AvmDocumentationGetCommand(
    ILogger<AvmDocumentationGetCommand> logger,
    IAvmDocsService avmDocsService) : BaseCommand<AvmDocumentationOptions>
{
    private readonly ILogger<AvmDocumentationGetCommand> _logger = logger;
    private readonly IAvmDocsService _avmDocsService = avmDocsService;

    public override string Id => "e5f6a7b8-c9d0-1234-ef01-456789012cde";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves the documentation (README.md) for a specific version of an Azure Verified Module (AVM).
        Returns the full module documentation including usage examples, input variables,
        output values, and resource descriptions. Use --module-name and --module-version
        to specify the module and version (e.g., --module-name avm-res-storage-storageaccount --module-version 0.4.0).
        """;

    public override string Title => "Get AVM Module Documentation";

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
        command.Options.Add(AzureTerraformOptionDefinitions.AvmModuleName);
        command.Options.Add(AzureTerraformOptionDefinitions.AvmModuleVersion);
    }

    protected override AvmDocumentationOptions BindOptions(ParseResult parseResult)
    {
        return new AvmDocumentationOptions
        {
            ModuleName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.AvmModuleName.Name),
            ModuleVersion = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.AvmModuleVersion.Name)
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
            var documentation = await _avmDocsService.GetDocumentationAsync(
                options.ModuleName!,
                options.ModuleVersion!,
                cancellationToken).ConfigureAwait(false);

            var result = new Models.AvmDocumentationResult
            {
                ModuleName = options.ModuleName!,
                ModuleVersion = options.ModuleVersion!,
                Documentation = documentation
            };

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AvmDocumentationResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving documentation for AVM module {ModuleName} version {Version}", options.ModuleName, options.ModuleVersion);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
