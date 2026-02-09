// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;

/// <summary>
/// Command to generate and download platform landing zone configurations, update parameters, check existing platform landing zones, and view status.
/// </summary>
public sealed class RequestCommand(ILogger<RequestCommand> logger)
    : SubscriptionCommand<RequestOptions>()
{
    private const string CommandTitle = "Platform Landing Zone Management";

    /// <inheritdoc/>
    public override string Id => "a7f3b8c1-9e2d-4f6a-8b3c-5d1e7f9a2c4b";

    /// <inheritdoc/>
    public override string Name => "request";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override string Description =>
        """
        Generate and download platform landing zone configurations for Azure Migrate projects.
        Updates parameters, check existing landing zones, and view parameters status.

        **Actions:**
        - check: Check if a platform landing zone already exists
        - update: Update all parameters for generation (collect ALL params in one call)
        - generate: Generate the platform landing zone
        - download: Download generated files to local workspace
        - status: View cached parameters

        **Context (required for all actions):**
        - subscription, resourceGroup, migrateProjectName

        **Generation Parameters (for 'update' action - collect ALL at once from user):**
        | Parameter | Options | Default |
        |-----------|---------|----------|
        | regionType | single, multi | single |
        | firewallType | azurefirewall, nva | azurefirewall |
        | networkArchitecture | hubspoke, vwan | hubspoke |
        | versionControlSystem | local, github, azuredevops | local |
        | regions | comma-separated (e.g., eastus,westus) | eastus |
        | environmentName | any string | prod |
        | organizationName | any string | contoso |
        | identitySubscriptionId | GUID | (uses main subscription) |
        | managementSubscriptionId | GUID | (uses main subscription) |
        | connectivitySubscriptionId | GUID | (uses main subscription) |

        **Workflow:**
        1. action='check' - See if one already exists
        2. action='update' with ALL parameters - Ask user to confirm defaults or provide values
        3. action='generate' - Create the landing zone
        4. action='download' - Get the files
        5. Extract zip to workspace root

        **IMPORTANT:** When using 'update', collect ALL parameters from the user in ONE call.
        Show them the defaults and ask which ones they want to change.
        """;

    /// <inheritdoc/>
    public override ToolMetadata Metadata => new()
    {
        Destructive = true,
        ReadOnly = false,
        LocalRequired = true,
        Idempotent = true,
        OpenWorld = false,
        Secret = false
    };

    /// <inheritdoc/>
    protected override void RegisterOptions(Command command)
    {
        base.RegisterOptions(command);
        command.Options.Add(OptionDefinitions.Common.ResourceGroup.AsRequired());
        command.Options.Add(PlatformLandingZoneOptionDefinitions.Action);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.RegionType);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.FireWallType);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.NetworkArchitecture);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.IdentitySubscriptionId);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.ManagementSubscriptionId);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.ConnectivitySubscriptionId);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.SecuritySubscriptionId);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.Regions);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.EnvironmentName);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.VersionControlSystem);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.OrganizationName);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.MigrateProjectName);
        command.Options.Add(PlatformLandingZoneOptionDefinitions.MigrateProjectResourceId);
    }

    /// <inheritdoc/>
    protected override RequestOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name)!;
        options.Action = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.Action.Name)!;
        options.RegionType = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.RegionType.Name);
        options.FireWallType = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.FireWallType.Name);
        options.NetworkArchitecture = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.NetworkArchitecture.Name);
        options.IdentitySubscriptionId = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.IdentitySubscriptionId.Name);
        options.ManagementSubscriptionId = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.ManagementSubscriptionId.Name);
        options.ConnectivitySubscriptionId = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.ConnectivitySubscriptionId.Name);
        options.SecuritySubscriptionId = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.SecuritySubscriptionId.Name);
        options.Regions = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.Regions.Name);
        options.EnvironmentName = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.EnvironmentName.Name);
        options.VersionControlSystem = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.VersionControlSystem.Name);
        options.OrganizationName = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.OrganizationName.Name);
        options.MigrateProjectName = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.MigrateProjectName.Name)!;
        options.MigrateProjectResourceId = parseResult.GetValueOrDefault<string>(PlatformLandingZoneOptionDefinitions.MigrateProjectResourceId.Name);
        return options;
    }

    /// <inheritdoc/>
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
            if (string.IsNullOrEmpty(options.Subscription))
            {
                throw new ArgumentException("Subscription is required.");
            }

            if (string.IsNullOrEmpty(options.ResourceGroup))
            {
                throw new ArgumentException("Resource group is required.");
            }

            if (string.IsNullOrEmpty(options.MigrateProjectName))
            {
                throw new ArgumentException("Migrate project name is required.");
            }

            var landingZoneContext = new PlatformLandingZoneContext(
                options.Subscription!,
                options.ResourceGroup!,
                options.MigrateProjectName);

            var action = options.Action?.ToLowerInvariant();

            var platformLandingZoneService = context.GetService<IPlatformLandingZoneService>();

            var result = action switch
            {
                "update" => await HandleUpdateActionAsync(platformLandingZoneService, landingZoneContext, options, cancellationToken),
                "check" => await HandleCheckActionAsync(platformLandingZoneService, landingZoneContext, cancellationToken),
                "generate" => await HandleGenerateActionAsync(platformLandingZoneService, landingZoneContext, cancellationToken),
                "download" => await HandleDownloadActionAsync(platformLandingZoneService, landingZoneContext, cancellationToken),
                "status" => HandleStatusAction(platformLandingZoneService, landingZoneContext),
                _ => throw new ArgumentException($"Invalid action '{options.Action}'. Valid actions are: update, check, generate, download, status.")
            };

            context.Response.Results = ResponseResult.Create(
                new RequestCommandResult(result),
                AzureMigrateJsonContext.Default.RequestCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static async Task<string> HandleUpdateActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        RequestOptions options,
        CancellationToken cancellationToken)
    {
        var updated = await service.UpdateParametersAsync(
            context,
            options.RegionType,
            options.FireWallType,
            options.NetworkArchitecture,
            options.IdentitySubscriptionId,
            options.ManagementSubscriptionId,
            options.ConnectivitySubscriptionId,
            options.Regions,
            options.EnvironmentName,
            options.VersionControlSystem,
            options.OrganizationName,
            cancellationToken);

        var message = $"Parameters updated successfully. Complete: {updated.IsComplete}";
        if (!updated.IsComplete)
        {
            var missing = service.GetMissingParameters(context);
            message += $"\nMissing required parameters: {string.Join(", ", missing)}";
        }

        return message;
    }

    private static async Task<string> HandleCheckActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var exists = await service.CheckExistingAsync(context, cancellationToken);

        if (exists)
        {
            return $"Platform Landing zone exists for Migrate project '{context.MigrateProjectName}' in resource group '{context.ResourceGroupName}'. You can download it using the 'download' action and then extract the files to the root of your local workspace. Delete the zip after extraction.";
        }

        return $"No Platform Landing zone found for Migrate project '{context.MigrateProjectName}' in resource group '{context.ResourceGroupName}'. " +
               "You can generate a new Platform Landing zone using the 'generate' action.";
    }

    private static async Task<string> HandleGenerateActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var missingParams = service.GetMissingParameters(context);
        if (missingParams.Count > 0)
        {
            var paramsNeeded = string.Join("\n  - ", missingParams);
            return $"Cannot generate platform landing zone. Please provide the following required parameters using the 'update' action first:\n  - {paramsNeeded}\n\n" +
                   $"Example: Use action='update' with these parameters:\n" +
                   $"  --region-type <single|multi>\n" +
                   $"  --firewall-type <azurefirewall|nva|none>\n" +
                   $"  --network-architecture <hubspoke|vwan>\n" +
                   $"  --identity-subscription-id <guid>\n" +
                   $"  --management-subscription-id <guid>\n" +
                   $"  --connectivity-subscription-id <guid>\n" +
                   $"  --regions <comma-separated regions>\n" +
                   $"  --environment-name <environment name>\n" +
                   $"  --version-control-system <local|github|azuredevops>";
        }

        var downloadUrl = await service.GenerateAsync(context, cancellationToken);

        if (string.IsNullOrEmpty(downloadUrl))
        {
            return "Platform Landing zone generation is in progress but the download URL is not yet available. " +
                   "The generation process may take several minutes to complete. " +
                   "Please wait a few minutes and then use the 'download' action again to check if the download URL is ready.";
        }

        return $"Platform Landing zone generated successfully. Download URL: {downloadUrl}\nUse 'download' action to retrieve the files.";
    }

    private static async Task<string> HandleDownloadActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var outputPath = Environment.CurrentDirectory;
        var filePath = await service.DownloadAsync(context, outputPath, cancellationToken);

        return $"Platform Landing zone downloaded successfully to: {filePath}. Extract the files to the root of the local workspace. To make changes to the platform landing zone, you can use the 'GetGuidance' command for guidance on modifying the configuration files. Delete the zip after extraction.";
    }

    private static string HandleStatusAction(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context)
    {
        return service.GetParameterStatus(context);
    }

    /// <summary>
    /// Result for the platform landing zone generate command.
    /// </summary>
    /// <param name="Message">The result message.</param>
    internal sealed record RequestCommandResult([property: JsonPropertyName("message")] string Message);
}
