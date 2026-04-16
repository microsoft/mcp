// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Azure.Mcp.Tools.FoundryExtensions.Models;
using Azure.Mcp.Tools.FoundryExtensions.Options;
using Azure.Mcp.Tools.FoundryExtensions.Options.Models;
using Azure.Mcp.Tools.FoundryExtensions.Services;
using Azure.ResourceManager;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Helpers;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.FoundryExtensions.Commands;

public sealed class KnowledgeIndexListCommand(IFoundryExtensionsService foundryExtensionsService) : GlobalCommand<KnowledgeIndexListOptions>
{
    private readonly IFoundryExtensionsService _foundryExtensionsService = foundryExtensionsService;

    private const string CommandTitle = "List Knowledge Indexes in Microsoft Foundry";

    public override string Id => "b2c3d4e5-2345-6789-bcde-f01234567890";

    public override string Name => "list";

    public override string Description =>
        """
        Retrieves a list of knowledge indexes from Microsoft Foundry.

        This function is used when a user requests information about the available knowledge indexes in Microsoft Foundry. It provides an overview of the knowledge bases and search indexes that are currently deployed and available for use with AI agents and applications.

        Requires the project endpoint URL (format: https://<resource>.services.ai.azure.com/api/projects/<project-name>).

        Usage:
            Use this function when a user wants to explore the available knowledge indexes in Microsoft Foundry. This can help users understand what knowledge bases are currently operational and how they can be utilized for retrieval-augmented generation (RAG) scenarios.

        Notes:
            - The indexes listed are knowledge indexes specifically created within Microsoft Foundry projects.
            - These indexes can be used with AI agents for knowledge retrieval and RAG applications.
            - The list may change as new indexes are created or existing ones are updated.
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
        command.Options.Add(FoundryExtensionsOptionDefinitions.EndpointOption);
        command.Validators.Add(commandResult =>
        {
            var endpointValue = commandResult.GetValueOrDefault(FoundryExtensionsOptionDefinitions.EndpointOption);
            if (string.IsNullOrWhiteSpace(endpointValue))
            {
                return;
            }

            ValidateFoundryEndpoint(endpointValue, commandResult);
        });
    }

    protected override KnowledgeIndexListOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.Endpoint = parseResult.GetValueOrDefault<string>(FoundryExtensionsOptionDefinitions.EndpointOption.Name);
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
            var indexes = await _foundryExtensionsService.ListKnowledgeIndexes(
                options.Endpoint!,
                options.Tenant,
                options.RetryPolicy,
                cancellationToken: cancellationToken);

            context.Response.Results = ResponseResult.Create(new(indexes ?? []), FoundryExtensionsJsonContext.Default.KnowledgeIndexListCommandResult);
        }
        catch (Exception ex)
        {
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static void ValidateFoundryEndpoint(string endpoint, System.CommandLine.Parsing.CommandResult commandResult)
    {
        ArmEnvironment[] clouds = [ArmEnvironment.AzurePublicCloud, ArmEnvironment.AzureChina, ArmEnvironment.AzureGovernment, ArmEnvironment.AzureGermany];
        string? lastError = null;

        foreach (var cloud in clouds)
        {
            try
            {
                EndpointValidator.ValidateAzureServiceEndpoint(endpoint, "foundry", cloud);
                return;
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
            }
        }

        commandResult.AddError(lastError ?? $"Invalid Foundry project endpoint: {endpoint}");
    }

    internal record KnowledgeIndexListCommandResult(IEnumerable<KnowledgeIndexInformation> Indexes);
}
