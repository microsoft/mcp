// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Azure.Mcp.Core.Commands.Subscription;
using Azure.Mcp.Core.Services.Azure.Subscription;
using Azure.Mcp.Tools.AzureMigrate.Constants;
using Azure.Mcp.Tools.AzureMigrate.Helpers;
using Azure.Mcp.Tools.AzureMigrate.Models;
using Azure.Mcp.Tools.AzureMigrate.Options.PlatformLandingZone;
using Azure.Mcp.Tools.AzureMigrate.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Mcp.Core.Commands;
using Microsoft.Mcp.Core.Models.Command;

namespace Azure.Mcp.Tools.AzureMigrate.Commands.PlatformLandingZone;

/// <summary>
/// Command to create, inspect and download Platform Landing Zones for an Azure Migrate project.
/// </summary>
[CommandMetadata(
    Id = "a7f3b8c1-9e2d-4f6a-8b3c-5d1e7f9a2c4b",
    Name = "request",
    Title = "Platform Landing Zone Management",
    Description = """
        Create, inspect and download Platform Landing Zones for an Azure Migrate project.

        A Platform Landing Zone is an ARM resource under a migrate project. Creating or updating it starts
        an asynchronous generation run that produces infrastructure-as-code output. The ARM call itself
        returns immediately; the generation run is tracked separately by 'status'.

        **Actions:**
        - createmigrateproject: Create a new Azure Migrate project (requires --location)
        - list: List the landing zones under the migrate project
        - get: Read the landing zone, its generation status, and its full effective configuration
        - create: Create the landing zone, or update the existing one, and start a generation run
        - wait: Poll until the generation run reaches a terminal status (Succeeded or Failed)
        - download: Download the generated output.zip once generation has succeeded

        **Context (required for all actions except createmigrateproject):**
        - --subscription, --resource-group, --migrate-project-name
        - A migrate project has exactly one Platform Landing Zone, always named 'default'. There is no
          name parameter, and a project cannot hold more than one landing zone.

        **Configuration parameters (for 'create'):**
        | Parameter | Values | Notes |
        |-----------|--------|-------|
        | --regions | comma-separated, e.g. eastus,westus2 | first is primary; two or more makes it multi-region |
        | --network-architecture | hubspoke, vwan | |
        | --firewall-type | azurefirewall, nva | 'none' is not supported by generation yet |
        | --bastion | enabled, disabled | |
        | --ddos | enabled, disabled | disabling removes the largest cost line item |
        | --private-dns | enabled, disabled | disabling also turns off centralized DNS resolution |
        | --express-route | enabled, disabled | enabling also requires --network-architecture |
        | --vpn-gateway | enabled | cannot be disabled yet; generation would fail |
        | --scale-tier | full, managementonly | managementonly omits connectivity entirely |
        | --version-control-system | local, github, azuredevops | |
        | --identity-subscription-id, --management-subscription-id, --connectivity-subscription-id, --security-subscription-id | GUID | supply together; any one supplied replaces the whole subscription block |
        | --parent-management-group-id | management group resource ID | create-only |
        | --organization-name, --service-name | any string | |

        **Do not invent values.** Every parameter is optional: the service applies its own defaults for
        anything omitted and echoes the full effective configuration back, which 'get' and 'create'
        display. Ask the user only about what they care about, send just that, and show them the echo.

        **Updating an existing landing zone is incremental.** The supplied parameters are layered over the
        current configuration, so `--ddos disabled` on its own is enough to turn DDoS protection off
        without restating anything else.

        **Turning off DDoS protection or Private DNS also switches off the matching Azure Landing Zones
        policy assignment**, because those components are policy-driven as well. This tool sends the
        required governance overrides automatically, so no extra parameter is needed.

        **Workflow:**
        1. action='list' or 'get' - see what already exists
        2. action='create' - create or update; only pass what the user asked for
        3. action='wait' - poll until generation finishes (this takes several minutes)
        4. action='download' - retrieve output.zip, then extract it into the workspace root and delete the zip
        """,
    Destructive = true,
    Idempotent = true,
    OpenWorld = false,
    ReadOnly = false,
    Secret = false,
    LocalRequired = true)]
public sealed class RequestCommand(
    ILogger<RequestCommand> logger,
    IPlatformLandingZoneService platformLandingZoneService,
    AzureMigrateProjectHelper azureMigrateProjectHelper,
    ISubscriptionResolver subscriptionResolver)
    : SubscriptionCommand<RequestOptions, RequestCommand.RequestCommandResult>(subscriptionResolver)
{
    private readonly IPlatformLandingZoneService _platformLandingZoneService = platformLandingZoneService;
    private readonly AzureMigrateProjectHelper _azureMigrateProjectHelper = azureMigrateProjectHelper;

    /// <inheritdoc/>
    public override async Task<CommandResponse> ExecuteAsync(
        CommandContext context,
        RequestOptions options,
        CancellationToken cancellationToken)
    {
        try
        {
            var landingZoneContext = new PlatformLandingZoneContext(
                options.Subscription!,
                options.ResourceGroup,
                options.MigrateProjectName,
                PlatformLandingZoneConstants.DefaultLandingZoneName);

            var result = options.Action.ToLowerInvariant() switch
            {
                "createmigrateproject" => await HandleCreateMigrateProjectActionAsync(_azureMigrateProjectHelper, options, cancellationToken),
                "list" => await HandleListActionAsync(_platformLandingZoneService, landingZoneContext, cancellationToken),
                "get" => await HandleGetActionAsync(_platformLandingZoneService, landingZoneContext, cancellationToken),
                "create" => await HandleCreateActionAsync(_platformLandingZoneService, landingZoneContext, options, cancellationToken),
                "wait" => await HandleWaitActionAsync(_platformLandingZoneService, landingZoneContext, options, cancellationToken),
                "download" => await HandleDownloadActionAsync(_platformLandingZoneService, landingZoneContext, options, cancellationToken),
                _ => throw new ArgumentException(
                    $"Invalid action '{options.Action}'. Valid actions are: createmigrateproject, list, get, create, wait, download.")
            };

            context.Response.Results = ResponseResult.Create(new(result), AzureMigrateJsonContext.Default.RequestCommandResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in {Operation}. Action: {Action}, ResourceGroup: {ResourceGroup}.", Name, options.Action, options.ResourceGroup);
            HandleException(context, ex);
        }

        return context.Response;
    }

    private static async Task<string> HandleListActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var landingZones = await service.ListAsync(context, cancellationToken);
        if (landingZones.Count == 0)
        {
            return $"No Platform Landing Zone exists under migrate project '{context.MigrateProjectName}' in resource group " +
                   $"'{context.ResourceGroupName}'. Use the 'create' action to create one.";
        }

        var builder = new StringBuilder();
        builder.AppendLine($"Platform Landing Zones under migrate project '{context.MigrateProjectName}':");
        foreach (var landingZone in landingZones)
        {
            builder.AppendLine(
                $"  - {landingZone.Name} (generation status: {landingZone.Status ?? "unknown"}, provisioning state: {landingZone.ProvisioningState ?? "unknown"})");
        }

        builder.Append("Use the 'get' action to see the full configuration.");
        return builder.ToString();
    }

    private static async Task<string> HandleGetActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        CancellationToken cancellationToken)
    {
        var landingZone = await service.GetAsync(context, cancellationToken);
        if (landingZone is null)
        {
            return $"No Platform Landing Zone exists under migrate project " +
                   $"'{context.MigrateProjectName}'. Use the 'create' action to create one.";
        }

        return Describe(landingZone, context);
    }

    private static async Task<string> HandleCreateActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        RequestOptions options,
        CancellationToken cancellationToken)
    {
        var landingZone = await service.CreateOrUpdateAsync(context, options, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine(
            $"Platform Landing Zone '{context.LandingZoneName}' was submitted under migrate project '{context.MigrateProjectName}'. " +
            "Generation runs in the background and typically takes several minutes.");
        builder.AppendLine(Describe(landingZone, context));
        builder.Append("Use the 'wait' action to poll until generation finishes, then the 'download' action to retrieve the output.");
        return builder.ToString();
    }

    private static async Task<string> HandleWaitActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        RequestOptions options,
        CancellationToken cancellationToken)
    {
        var timeout = options.TimeoutMinutes is > 0
            ? TimeSpan.FromMinutes(options.TimeoutMinutes.Value)
            : PlatformLandingZoneConstants.DefaultWaitTimeout;

        var landingZone = await service.WaitForTerminalAsync(context, timeout, cancellationToken);
        var summary = Describe(landingZone, context);

        if (string.Equals(landingZone.Status, PlatformLandingZoneConstants.StatusFailed, StringComparison.OrdinalIgnoreCase))
        {
            return $"Platform Landing Zone '{context.LandingZoneName}' failed to generate.\n{summary}";
        }

        return $"Platform Landing Zone '{context.LandingZoneName}' finished generating.\n{summary}\n" +
               "Use the 'download' action to retrieve the generated output.";
    }

    private static async Task<string> HandleDownloadActionAsync(
        IPlatformLandingZoneService service,
        PlatformLandingZoneContext context,
        RequestOptions options,
        CancellationToken cancellationToken)
    {
        var files = await service.DownloadAsync(
            context,
            Environment.CurrentDirectory,
            options.IncludeDesignDocument,
            cancellationToken);

        return $"Platform Landing Zone '{context.LandingZoneName}' downloaded to:\n  - {string.Join("\n  - ", files)}\n" +
               "Extract the archive into the root of the local workspace and delete the zip afterwards. " +
               "To change the landing zone, prefer re-running the 'create' action with the parameters you want to change; " +
               "use the 'getguidance' command for changes that are not exposed as parameters.";
    }

    private static async Task<string> HandleCreateMigrateProjectActionAsync(
        AzureMigrateProjectHelper azureMigrateProjectHelper,
        RequestOptions options,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(options.Location))
        {
            throw new ArgumentException("Location is required for creating an Azure Migrate project. Specify the Azure region (e.g., 'eastus', 'westus2').");
        }

        var result = await azureMigrateProjectHelper.CreateAzureMigrateProjectAsync(
            options.MigrateProjectName,
            options.ResourceGroup,
            options.Location,
            options.Subscription!,
            options.Tenant,
            cancellationToken);

        if (!result.HasData)
        {
            return $"Failed to create Azure Migrate project '{options.MigrateProjectName}'. The operation completed but no data was returned.";
        }

        return $"Azure Migrate project '{result.Name}' created successfully in resource group '{options.ResourceGroup}' at location '{result.Location}'.\n" +
               $"Resource ID: {result.Id}\n" +
               "You can now use the 'list', 'get', 'create', 'wait' and 'download' actions to manage a Platform Landing Zone.";
    }

    private static string Describe(PlatformLandingZoneView landingZone, PlatformLandingZoneContext context)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Name: {landingZone.Name ?? context.LandingZoneName}");
        builder.AppendLine($"Provisioning state: {landingZone.ProvisioningState ?? "unknown"}");
        builder.AppendLine($"Generation status: {landingZone.Status ?? "unknown"}");

        if (!string.IsNullOrWhiteSpace(landingZone.ArtifactId))
        {
            builder.AppendLine($"Artifact: {landingZone.ArtifactId}");
        }

        if (!string.IsNullOrWhiteSpace(landingZone.EffectiveProperties))
        {
            builder.AppendLine("Effective configuration (includes every value defaulted by the service):");
            builder.AppendLine(landingZone.EffectiveProperties);
        }

        return builder.ToString().TrimEnd();
    }

    /// <summary>
    /// Result for the platform landing zone request command.
    /// </summary>
    /// <param name="Message">The result message.</param>
    public sealed record RequestCommandResult(string Message);
}
