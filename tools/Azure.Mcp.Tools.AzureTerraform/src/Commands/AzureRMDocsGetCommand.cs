// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using Azure.Mcp.Tools.AzureTerraform.Options;
using Azure.Mcp.Tools.AzureTerraform.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureTerraform.Commands;

public sealed class AzureRMDocsGetCommand(
    ILogger<AzureRMDocsGetCommand> logger,
    IAzureRMDocsService docsService) : BaseCommand<AzureRMDocsOptions>
{
    private readonly ILogger<AzureRMDocsGetCommand> _logger = logger;
    private readonly IAzureRMDocsService _docsService = docsService;

    public override string Id => "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

    public override string Name => "get";

    public override string Description =>
        """
        Retrieves comprehensive AzureRM Terraform provider documentation for a specified resource type.
        Returns the resource summary, arguments with descriptions and requirements, attributes,
        usage examples, and important notes. Use --resource-type to specify the resource
        (e.g., azurerm_resource_group). Optionally filter by --doc-type (resource or data-source),
        --argument, or --attribute.
        """;

    public override string Title => "Get AzureRM Provider Documentation";

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
        command.Options.Add(AzureTerraformOptionDefinitions.ResourceType);
        command.Options.Add(AzureTerraformOptionDefinitions.DocType);
        command.Options.Add(AzureTerraformOptionDefinitions.ArgumentName);
        command.Options.Add(AzureTerraformOptionDefinitions.AttributeName);
    }

    protected override AzureRMDocsOptions BindOptions(ParseResult parseResult)
    {
        return new AzureRMDocsOptions
        {
            ResourceType = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.ResourceType.Name),
            DocType = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.DocType.Name),
            ArgumentName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.ArgumentName.Name),
            AttributeName = parseResult.GetValueOrDefault<string>(AzureTerraformOptionDefinitions.AttributeName.Name)
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
            var result = await _docsService.GetDocumentationAsync(
                options.ResourceType!,
                options.DocType ?? "resource",
                options.ArgumentName,
                options.AttributeName,
                cancellationToken).ConfigureAwait(false);

            context.Response.Status = HttpStatusCode.OK;
            context.Response.Results = ResponseResult.Create(result, AzureTerraformJsonContext.Default.AzureRMDocsResult);
            context.Response.Message = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AzureRM documentation for {ResourceType}", options.ResourceType);
            HandleException(context, ex);
        }

        return context.Response;
    }
}
