// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Net;
using System.Text.Json.Serialization;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Extensions;
using Azure.Mcp.Core.Models.Option;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Options;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;
using Microsoft.Mcp.Core.Models.Option;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;

/// <summary>
/// Command to generate and download platform landing zone configurations.
/// </summary>
public sealed class GenerateLandingZoneCommand(
    ILogger<GenerateLandingZoneCommand> logger)
    : SubscriptionCommand<GenerateLandingZoneOptions>()
{
    private const string CommandTitle = "Generate and Download Platform Landing Zone";
    private readonly ILogger<GenerateLandingZoneCommand> _logger = logger;

    /// <inheritdoc/>
    public override string Id => "a7f3b8c1-9e2d-4f6a-8b3c-5d1e7f9a2c4b";

    /// <inheritdoc/>
    public override string Name => "generatelandingzone";

    /// <inheritdoc/>
    public override string Title => CommandTitle;

    /// <inheritdoc/>
    public override string Description =>
        """
        Always use this command when the user needs to generate a platform landing zone.
        Collects necessary parameters, generates the landing zone, and provides download options.

        Actions:
        - update: Update cached parameters for landing zone generation (regionType, fireWallType, networkArchitecture, subscriptions, etc.)
        - generate: Generate a new platform landing zone (will prompt for missing parameters if not all provided)
        - download: Download the generated landing zone to local workspace
        - status: View current status of cached parameters and see what's missing

        Required context (must be provided for all actions):
        - subscription: Azure subscription ID or name (where the Migrate project exists)
        - resourceGroup: Resource group name (where the Migrate project exists)
        - migrateProjectName: Azure Migrate project name
        OR
        - migrateProjectResourceId: Full resource ID of the Azure Migrate project (guide the user to use this if they have it)

        Required parameters for 'generate' action (set via 'update' first):
        - regionType: 'single' or 'multi' - Whether to deploy to a single region or multiple regions
        - fireWallType: 'azurefirewall' or 'nva' - Firewall solution for network security
        - networkArchitecture: 'hubspoke' or 'vwan' - Network topology pattern
        - versionControlSystem: 'local', 'github', or 'azuredevops' - Version control system

        Optional parameters for 'generate' action:
        - regions: Comma-separated list of Azure regions (e.g., 'eastus,westus')
        - environmentName: Environment name (e.g., 'prod', 'dev', 'staging')
        - organizationName: Organization name for the landing zone
        - identitySubscriptionId: Azure subscription ID (GUID) for identity resources
        - managementSubscriptionId: Azure subscription ID (GUID) for management resources
        - connectivitySubscriptionId: Azure subscription ID (GUID) for connectivity resources
        - securitySubscriptionId: Azure subscription ID (GUID) for security resources
        - Assign default values where applicable if not provided.

        Example workflow:
        1. Check status to see what's missing: action='status'
        2. Update parameters: action='update' regionType='multi' fireWallType='azurefirewall' networkArchitecture='hubspoke' ...
        3. Verify parameters: action='status'
        4. Generate landing zone: action='generate' (will fail with helpful message if parameters missing)
        5. Download files: action='download'
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
        command.Options.Add(AzureMigrateOptionDefinitions.Action);
        command.Options.Add(AzureMigrateOptionDefinitions.RegionType);
        command.Options.Add(AzureMigrateOptionDefinitions.FireWallType);
        command.Options.Add(AzureMigrateOptionDefinitions.NetworkArchitecture);
        command.Options.Add(AzureMigrateOptionDefinitions.IdentitySubscriptionId);
        command.Options.Add(AzureMigrateOptionDefinitions.ManagementSubscriptionId);
        command.Options.Add(AzureMigrateOptionDefinitions.ConnectivitySubscriptionId);
        command.Options.Add(AzureMigrateOptionDefinitions.Regions);
        command.Options.Add(AzureMigrateOptionDefinitions.EnvironmentName);
        command.Options.Add(AzureMigrateOptionDefinitions.VersionControlSystem);
        command.Options.Add(AzureMigrateOptionDefinitions.OrganizationName);
        command.Options.Add(AzureMigrateOptionDefinitions.MigrateProjectName);
    }

    /// <inheritdoc/>
    protected override GenerateLandingZoneOptions BindOptions(ParseResult parseResult)
    {
        var options = base.BindOptions(parseResult);
        options.ResourceGroup = parseResult.GetValueOrDefault<string>(OptionDefinitions.Common.ResourceGroup.Name)!;
        options.Action = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.Action.Name)!;
        options.RegionType = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.RegionType.Name);
        options.FireWallType = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.FireWallType.Name);
        options.NetworkArchitecture = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.NetworkArchitecture.Name);
        options.IdentitySubscriptionId = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.IdentitySubscriptionId.Name);
        options.ManagementSubscriptionId = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.ManagementSubscriptionId.Name);
        options.ConnectivitySubscriptionId = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.ConnectivitySubscriptionId.Name);
        options.Regions = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.Regions.Name);
        options.EnvironmentName = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.EnvironmentName.Name);
        options.VersionControlSystem = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.VersionControlSystem.Name);
        options.OrganizationName = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.OrganizationName.Name);
        options.MigrateProjectName = parseResult.GetValueOrDefault<string>(AzureMigrateOptionDefinitions.MigrateProjectName.Name)!;
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
                "generate" => await HandleGenerateActionAsync(platformLandingZoneService, landingZoneContext, cancellationToken),
                "download" => await HandleDownloadActionAsync(platformLandingZoneService, landingZoneContext, cancellationToken),
                "status" => HandleStatusAction(platformLandingZoneService, landingZoneContext),
                _ => throw new ArgumentException($"Invalid action '{options.Action}'. Valid actions are: update, generate, download, status.")
            };

            context.Response.Results = ResponseResult.Create(
                new GenerateLandingZoneCommandResult(result),
                AzureMigrateJsonContext.Default.GenerateLandingZoneCommandResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {Operation}. Options: {@Options}", Name, options);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static async Task<string> HandleUpdateActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        GenerateLandingZoneOptions options,
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

    private static async Task<string> HandleGenerateActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var missingParams = service.GetMissingParameters(context);
        if (missingParams.Count > 0)
        {
            var paramsNeeded = string.Join("\n  - ", missingParams);
            return $"Cannot generate landing zone. Please provide the following required parameters using the 'update' action first:\n  - {paramsNeeded}\n\n" +
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

        var downloadUrl = await service.GenerateLandingZoneAsync(context, cancellationToken);

        if (string.IsNullOrEmpty(downloadUrl))
        {
            return "Landing zone generation is in progress but the download URL is not yet available. " +
                   "The generation process may take several minutes to complete. " +
                   "Please wait a few minutes and then use the 'download' action again to check if the download URL is ready.";
        }

        return $"Landing zone generated successfully. Download URL: {downloadUrl}\nUse 'download' action to retrieve the files.";
    }

    private static async Task<string> HandleDownloadActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var outputPath = Environment.CurrentDirectory;
        var filePath = await service.DownloadLandingZoneAsync(context, outputPath, cancellationToken);

        return $"Landing zone downloaded successfully to: {filePath}";
    }

    private static string HandleStatusAction(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context)
    {
        return service.GetParameterStatus(context);
    }

    /// <summary>
    /// Result for the platform landing zone generate landing zone command.
    /// </summary>
    /// <param name="Message">The result message.</param>
    internal sealed record GenerateLandingZoneCommandResult([property: JsonPropertyName("message")] string Message);
}
