// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Azure.Mcp.Tools.AzureMigrate.Constants;

/// <summary>
/// Constants for Platform Landing Zone operations and guidance.
/// </summary>
internal static class PlatformLandingZoneConstants
{
    /// <summary>
    /// Base URL for Azure Landing Zone scenario documentation.
    /// </summary>
    public const string ScenarioDocsBaseUrl = "https://raw.githubusercontent.com/Azure/Azure-Landing-Zones/main/docs/content/accelerator/startermodules/terraform-platform-landing-zone/options";

    /// <summary>
    /// Base URL for Azure Landing Zone archetype definitions.
    /// </summary>
    public const string ArchetypeDefinitionsBaseUrl = "https://raw.githubusercontent.com/Azure/Azure-Landing-Zones-Library/main/platform/alz/archetype_definitions";

    /// <summary>
    /// Azure Resource Manager base URL.
    /// </summary>
    public const string ArmBaseUrl = "https://management.azure.com";

    /// <summary>
    /// Azure Management API scope.
    /// </summary>
    public const string ManagementScope = "https://management.azure.com/.default";

    /// <summary>
    /// API version for Azure Migrate operations.
    /// </summary>
    public const string ApiVersion = "2020-06-01-preview";

    /// <summary>
    /// Cache expiration time for policy location data.
    /// </summary>
    public static readonly TimeSpan PolicyCacheExpiry = TimeSpan.FromHours(6);

    /// <summary>
    /// Archetype definition file names.
    /// </summary>
    public static readonly string[] ArchetypeDefinitionFiles =
    [
        "connectivity.alz_archetype_definition.json",
        "corp.alz_archetype_definition.json",
        "decommissioned.alz_archetype_definition.json",
        "identity.alz_archetype_definition.json",
        "landing_zones.alz_archetype_definition.json",
        "management.alz_archetype_definition.json",
        "online.alz_archetype_definition.json",
        "platform.alz_archetype_definition.json",
        "root.alz_archetype_definition.json",
        "sandbox.alz_archetype_definition.json",
        "security.alz_archetype_definition.json"
    ];
}
