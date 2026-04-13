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

public sealed class AzApiDocsGetCommand(
    ILogger<AzApiDocsGetCommand> logger,
    IAzApiDocsService docsService,
    IAzApiExamplesService examplesService) : BaseCommand<AzApiDocsOptions>
{
    private readonly ILogger<AzApiDocsGetCommand> _logger = logger;
    private readonly IAzApiDocsService _docsService = docsService;
    private readonly IAzApiExamplesService _examplesService = examplesService;

    public override string Id => "f6a7b8c9-d0e1-2345-bcde-678901234ef0";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves AzAPI Terraform provider documentation and schema for a specified Azure resource type.
        Returns the resource schema in HCL format suitable for azapi_resource blocks, including property
        definitions with types and requirements, parent resource information, and Terraform usage examples.
        Use --resource-type to specify the Azure resource type in ARM format
        (e.g., Microsoft.Compute/virtualMachines, Microsoft.Storage/storageAccounts).
        Optionally specify --api-version to target a specific API version.
        This tool reuses Azure Bicep type definitions to generate accurate AzAPI schemas.
        """;

    public override string Title => "Get AzAPI Provider Documentation";

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
        command.Options.Add(AzureTerraformOptionDefinitions.AzApiResourceType.AsRequired());
        command.Options.Add(AzureTerraformOptionDefinitions.ApiVersion.AsOptional());
    }

    protected override AzApiDocsOptions BindOptions(ParseResult parseResult)
    {
        return new AzApiDocsOptions
        {
            ResourceType = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.AzApiResourceType.Name),
            ApiVersion = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.ApiVersion.Name)
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
            var result = _docsService.GetDocumentation(options.ResourceType!, options.ApiVersion);

            var examples = await _examplesService.GetExamplesAsync(
                options.ResourceType!,
                cancellationToken).ConfigureAwait(false);

            if (examples.Count > 0)
            {
                result.Examples = examples;
            }

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AzApiDocsResult);
            context.Response.Message = string.Empty;

            context.Activity
                ?.AddTag(AzureTerraformTelemetryTags.ToolArea, "azapi")
                .AddTag(AzureTerraformTelemetryTags.ResourceType, options.ResourceType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AzAPI documentation for {ResourceType}", options.ResourceType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
